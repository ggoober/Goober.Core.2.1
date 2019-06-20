using EasyNetQ;
using EasyNetQ.Consumer;
using Goober.RabbitMq.Internal;
using Goober.RabbitMq.Internal.Implementation;
using Goober.RabbitMq.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.RabbitMq.BackgroundWorkers
{
    public class RabbitMqConsumerBackgroundWorker : IHostedService
    {
        #region fields

        private readonly ConcurrentDictionary<Type, ConsumerConnectionModel> _busDict = new ConcurrentDictionary<Type, ConsumerConnectionModel>();
        private readonly RabbitMqClientOptions _options;

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private ILogger _logger;

        private Stopwatch _serviceWatch = new Stopwatch();

        #endregion

        #region public properties

        public DateTime? StartDateTime { get; protected set; }

        public DateTime? StopDateTime { get; protected set; }

        public bool IsRunning { get; protected set; } = false;

        public TimeSpan ServiceUpTime => _serviceWatch.Elapsed;

        #endregion

        #region ctor

        public RabbitMqConsumerBackgroundWorker(ILogger<RabbitMqConsumerBackgroundWorker> logger,
            IServiceProvider serviceProvider,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMqClientOptions> rabbitMqClientOptionsAccessor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _options = rabbitMqClientOptionsAccessor.Value;
        }

        #endregion

        public List<KeyValuePair<Type, ConsumerConnectionModel>> GetConsumers()
        {
            return _busDict.ToList();
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsRunning == true)
            {
                throw new InvalidOperationException($"RabbitMqConsumerBackgroundWorker start failed, task already executing...");
            }

            _logger.LogInformation($"RabbitMqConsumerBackgroundWorker is starting...");

            StartDateTime = DateTime.Now;
            _serviceWatch.Start();
            IsRunning = true;

            try
            {
                StartListening();

                _logger.LogInformation($"RabbitMqConsumerBackgroundWorker has started.");
            }
            catch (Exception exc)
            {
                _logger.LogCritical(exc, $"RabbitMqConsumerBackgroundWorker start fail");
                IsRunning = false;
            }

            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var iConnection in _busDict)
            {
                iConnection.Value.Bus.Dispose();
                iConnection.Value.SubscriptionResult.Dispose();
            }

            _busDict.Clear();

            _logger.LogInformation($"RabbitMqConsumerBackgroundWorker stoping...");
            StopDateTime = DateTime.Now;

            IsRunning = false;
            _logger.LogInformation($"RabbitMqConsumerBackgroundWorker stopped");

            return Task.CompletedTask;
        }

        public void StartListening()
        {
            if (_options.Consumers == null)
                return;

            foreach (var iConsumer in _options.Consumers)
            {
                var messageType = Type.GetType(iConsumer.MessageTypeFullName);
                if (messageType == null)
                    throw new InvalidOperationException($"Failed get message type for {iConsumer.MessageTypeFullName}");

                _busDict.GetOrAdd(messageType, (_) => { return Subscribe(iConsumer); });
            }
        }

        private void CheckHandlerIsResolvable(Type handlerType)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var handler = scope.ServiceProvider.GetService(handlerType);
                if (handler == null)
                {
                    throw new InvalidOperationException("Unable to resolve message handler: " + handlerType.FullName);
                }
            }
        }

        private ConsumerConnectionModel Subscribe(RabbitMqConsumerOptions consumerOptions)
        {
            var messageType = Type.GetType(consumerOptions.MessageTypeFullName);
            if (messageType == null)
                throw new InvalidOperationException($"Failed get message type for {consumerOptions.MessageTypeFullName}");

            var handlerType = Type.GetType(consumerOptions.HandlerTypeFullName);
            if (handlerType == null)
                throw new InvalidOperationException($"Failed get handler type for {consumerOptions.HandlerTypeFullName}");

            CheckHandlerIsResolvable(handlerType);

            var cc = new ConsumerConnectionModel
            {
                MaxParallelHandlers = consumerOptions.MaxParallelHandlers,
                MessageProccessRetryDelayInMilliseconds = consumerOptions.MessageProccessRetryDelayInMilliseconds,
                MessageProcessRetryCount = consumerOptions.MessageProcessRetryCount,
                PrefetchCount = consumerOptions.PrefetchCount
            };

            var handlerInvokerType = typeof(MessageHandlerInvoker<,>).MakeGenericType(messageType, handlerType);
            cc.MessageHandlerInvoker = (IMessageHandlerInvoker)Activator.CreateInstance(handlerInvokerType,
                                    new object[] {
                                        _serviceProvider,
                                        cc.MessageProcessRetryCount,
                                        TimeSpan.FromMilliseconds(cc.MessageProccessRetryDelayInMilliseconds)
                                    });

            var subscribeAsync = typeof(IBus)
                .GetMethods()
                .FirstOrDefault(x => x.Name == nameof(IBus.SubscribeAsync) && x.GetParameters().Length == 2)?
                .MakeGenericMethod(messageType);

            if (subscribeAsync == null)
            {
                throw new InvalidOperationException("Unable to subscribe due to: IBus.SubscribeAsync not found");
            }

            var bus = CreateBus(messageType);

            var onMessage = cc.MessageHandlerInvoker.CreateHandlerDelegate();
            cc.SubscriptionResult = (ISubscriptionResult)subscribeAsync.Invoke(bus, new object[] { _options.AppName, onMessage });

            return cc;
        }

        private IBus CreateBus(Type type)
        {
            var connectionConfiguration = new ConnectionConfiguration()
            {
                PublisherConfirms = true,
                PersistentMessages = true,

                Password = _options.Password,
                UserName = _options.UserName,
                VirtualHost = _options.VirtualHost,

                Name = $"[C]{_options.AppName}:{type.Name}",

                PrefetchCount = 10, // _options.PrefetchCount,

                Hosts = new List<HostConfiguration>
                {
                    new HostConfiguration
                    {
                        Host = _options.Host,
                        Port = _options.Port
                    }
                }
            };

            return RabbitHutch.CreateBus(connectionConfiguration, serviceRegister =>
            {
                serviceRegister.Register(_ => _options);
                serviceRegister.Register<IConsumerErrorStrategy, ConsumerErrorStrategy>();
                serviceRegister.Register<IHandlerRunner, LimitedHandlerRunner>();
                serviceRegister.Register<ILoggerFactory>(_ => _serviceProvider.GetRequiredService<ILoggerFactory>());
            });
        }
    }
}

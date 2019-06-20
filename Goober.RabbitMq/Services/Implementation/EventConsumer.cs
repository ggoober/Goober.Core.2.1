using EasyNetQ;
using EasyNetQ.Consumer;
using Goober.RabbitMq.Abstractions;
using Goober.RabbitMq.Internal;
using Goober.RabbitMq.Internal.Implementation;
using Goober.RabbitMq.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Goober.RabbitMq.Services.Implementation
{
    class EventConsumer: IEventConsumer
    {
        private readonly Dictionary<Type, Type> _subscriptions = new Dictionary<Type, Type>();
        private readonly ConcurrentDictionary<Type, IBus> _busDict = new ConcurrentDictionary<Type, IBus>();

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMqClientOptions _options;

        public EventConsumer(IServiceProvider serviceProvider,
            IOptions<RabbitMqClientOptions> rabbitMqClientOptionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _options = rabbitMqClientOptionsAccessor.Value;
        }

        public void AddSubscription<TEvent, TEventHandler>()
            where TEvent : class
            where TEventHandler : class, IMessageHandler<TEvent>
        {
            _subscriptions.Add(typeof(TEvent), typeof(TEventHandler));
        }

        public void Dispose()
        {
            _subscriptions.Clear();
        }

        public void StartListening()
        {
            foreach (var subscription in _subscriptions)
            {
                CheckHandlerIsResolvable(subscription.Value);
                Subscribe(subscription.Key, subscription.Value);
            }
        }

        private void CheckHandlerIsResolvable(Type handlerType)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                if (handler == null)
                {
                    throw new InvalidOperationException("Unable to resolve message handler: " + handlerType.FullName);
                }
            }
        }

        private void Subscribe(Type messageType, Type handlerType)
        {
            // библиотека, которая используется здесь для работы с RabbitMQ, имеет 2 варианта программного интерфейса. 
            // Первый вариант очень простой. Второй вариант Advanced - немного сложнее, но требует много ручной работы.
            // Здесь по сути необходимо нечто среднее между ними, Advanced оверхед, но и простой неудовлетворяет нуждам. 
            // Принимая во внимание, что данный код - код подписки - будет исполнен единожды при старте, было принято решение использовать 
            // простой интерфейс + рефлексию. На произоводительности это не скажется по выше упомянутой причине.

            var handlerInvokerType = typeof(MessageHandlerInvoker<,>).MakeGenericType(messageType, handlerType);
            var handlerInvoker = (IMessageHandlerInvoker)Activator.CreateInstance(handlerInvokerType, new object[] { _serviceProvider });

            var subscribeAsync = typeof(IBus)
                .GetMethods()
                .FirstOrDefault(x => x.Name == nameof(IBus.SubscribeAsync) && x.GetParameters().Length == 2)?
                .MakeGenericMethod(messageType);

            if (subscribeAsync == null)
            {
                throw new InvalidOperationException("Unable to subscribe due to: IBus.SubscribeAsync not found");
            }

            var onMessage = handlerInvoker.CreateHandlerDelegate();

            var bus =_busDict.GetOrAdd(messageType, CreateBus(messageType));

            var subscriptionResult = (ISubscriptionResult)subscribeAsync.Invoke(bus, new object[] { _options.AppName, onMessage });
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

                PrefetchCount = 10, //_options.PrefetchCount,

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

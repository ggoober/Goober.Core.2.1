using EasyNetQ;
using Goober.RabbitMq.Internal;
using Goober.RabbitMq.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Services.Implementation
{
    class RabbitMqMessageProducer : IRabbitMqMessageProducer
    {
        private static uint _defaultPublishRetryCount = 10;
        private static uint _defaultPublishRetryInterval = 1000;

        private readonly AsyncPolicy _defaultRetryPolicyAsync;
        private readonly Policy _defaultRetryPolicy;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqMessageProducer> _logger;
        private readonly RabbitMqClientOptions _options;
        private readonly ConcurrentDictionary<Type, ProducerConnectionModel> _busDict = new ConcurrentDictionary<Type, ProducerConnectionModel>();

        public RabbitMqMessageProducer(IServiceProvider serviceProvider,
            ILogger<RabbitMqMessageProducer> logger,
            IOptions<RabbitMqClientOptions> rabbitMqClientOptionsAccessor)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            _options = rabbitMqClientOptionsAccessor.Value;

            _defaultRetryPolicyAsync = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: (int)_defaultPublishRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(_defaultPublishRetryInterval));

            _defaultRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    retryCount: (int)_defaultPublishRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(_defaultPublishRetryInterval));
        }

        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : class
        {
            var messageType = message.GetType();

            try
            {
                var producerConnection = _busDict.GetOrAdd(messageType, key => { return GetProducerConnection(key); });

                await producerConnection.RetryPolicyAsync.ExecuteAsync(async () =>
                {
                    await producerConnection.Bus.PublishAsync(message);
                });
            }
            catch (Exception exc)
            {
                _logger.LogError(message: $"Failed to push message {JsonConvert.SerializeObject(message)}", exception: exc);
                throw;
            }
        }

        public void Publish<TMessage>(TMessage message) where TMessage : class
        {
            var messageType = message.GetType();

            try
            {
                var producerConnection = _busDict.GetOrAdd((Type)messageType, key => { return GetProducerConnection(key); });
                producerConnection.RetryPolicy.Execute(() =>
                {
                    producerConnection.Bus.Publish(message);
                });
            }
            catch (Exception exc)
            {
                _logger.LogError(message: $"Failed to push message {JsonConvert.SerializeObject(message)}", exception: exc);
                throw;
            }
        }

        public void Dispose()
        {
            foreach (var iBus in _busDict)
            {
                iBus.Value.Bus.Dispose();
            }

            _busDict.Clear();
        }

        private ProducerConnectionModel GetProducerConnection(Type messageType)
        {
            var ret = new ProducerConnectionModel { };
            var producerOptions = _options.Producers?.FirstOrDefault(x => x.GetMessageType() == messageType);
            if (producerOptions == null)
            {
                ret.PublishRetryCount = _defaultPublishRetryCount;
                ret.PublishRetryIntervalInMilliseconds = _defaultPublishRetryInterval;
                ret.RetryPolicy = _defaultRetryPolicy;
                ret.RetryPolicyAsync = _defaultRetryPolicyAsync;
            }
            else
            {
                ret.PublishRetryCount = producerOptions.PublishRetryCount;
                ret.PublishRetryIntervalInMilliseconds = producerOptions.PublishRetryIntervalInMilliseconds;

                ret.RetryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetry(
                            retryCount: (int)producerOptions.PublishRetryCount,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(producerOptions.PublishRetryIntervalInMilliseconds));

                ret.RetryPolicyAsync = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(
                            retryCount: (int)producerOptions.PublishRetryCount,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(producerOptions.PublishRetryIntervalInMilliseconds));
            }

            ret.Bus = CreateBus(messageType);

            return ret;
        }

        private IBus CreateBus(Type messageType)
        {
            var connectionConfiguration = new ConnectionConfiguration()
            {
                Password = _options.Password,
                UserName = _options.UserName,
                VirtualHost = _options.VirtualHost,

                Name = $"[P]{_options.AppName}:{messageType.Name}",

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
                serviceRegister.Register<ILoggerFactory>(_ => _serviceProvider.GetRequiredService<ILoggerFactory>());
            });
        }
    }
}

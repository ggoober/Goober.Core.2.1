using Goober.RabbitMq.Abstractions;
using Goober.RabbitMq.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Internal.Implementation
{
    class MessageHandlerInvoker<TMessage, TMessageHandler> : IMessageHandlerInvoker
        where TMessage : class
        where TMessageHandler : class, IMessageHandler<TMessage>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MessageHandlerInvoker<TMessage, TMessageHandler>> _logger;
        private readonly RabbitMqClientOptions _options;
        private readonly AsyncPolicy _retryPolicy;

        public MessageHandlerInvoker(IServiceProvider serviceProvider)
        {
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _logger = serviceProvider.GetRequiredService<ILogger<MessageHandlerInvoker<TMessage, TMessageHandler>>>();
            _options = serviceProvider.GetRequiredService<IOptions<RabbitMqClientOptions>>().Value;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3, //_options.ConsumerRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(1) //_options.ConsumerRetryInterval
                    );
        }

        public Delegate CreateHandlerDelegate()
        {
            return (Func<TMessage, Task>)ProcessWithRetryAsync;
        }

        public async Task ProcessWithRetryAsync(TMessage message)
        {
            await _retryPolicy.ExecuteAsync(() => ProcessAsync(message));
        }

        private async Task ProcessAsync(TMessage message)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var handler = ResolveHandler(scope);
                    await handler.ProcessAsync(message);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(new EventId(0), ex, ex.Message);
                    throw;
                }
            }
        }

        private TMessageHandler ResolveHandler(IServiceScope scope)
        {
            var handler = scope.ServiceProvider.GetRequiredService<TMessageHandler>();

            if (handler == null)
            {
                throw new InvalidOperationException("Unable to resolve message handler: " + typeof(TMessageHandler).FullName);
            }

            return handler;
        }
    }
}

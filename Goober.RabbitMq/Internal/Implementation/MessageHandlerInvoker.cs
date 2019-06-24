using Goober.RabbitMq.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Internal.Implementation
{
    class MessageHandlerInvoker<TMessage, TMessageHandler> : IMessageHandlerInvoker
        where TMessage : class
        where TMessageHandler : class, IMessageHandler<TMessage>
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private SemaphoreSlim _semaphoreSlim;

        #region metrics

        private long _messagesProcessingAttemptsCount;
        public long MessagesProcessingAttemptsCount => _messagesProcessingAttemptsCount;

        private long _messagesSuccessProcessedCount;
        public long MessagesSuccessProcessedCount => _messagesSuccessProcessedCount;

        private long _messagesErrorProcessedCount;
        public long MessagesErrorProcessedCount => _messagesErrorProcessedCount;

        private long _lastMessageProcessingAttemptDate;
        public DateTime? LastMessageProcessingAttemptDate
        {
            get
            {
                if (_lastMessageProcessingAttemptDate == 0)
                    return null;

                return DateTime.FromBinary(_lastMessageProcessingAttemptDate);
            }
        }

        private long _lastMessageSuccessProcessedDate;
        public DateTime? LastMessageSuccessProcessedDate
        {
            get
            {
                if (_lastMessageSuccessProcessedDate == 0)
                    return null;

                return DateTime.FromBinary(_lastMessageSuccessProcessedDate);
            }
        }

        private long _sumMessagesProcessingDurationInMilliseconds;
        public long? AvgMessagesProcessingDurationInMilliseconds
        {
            get
            {
                if (_messagesSuccessProcessedCount == 0)
                    return null;

                return _sumMessagesProcessingDurationInMilliseconds / _messagesSuccessProcessedCount;
            }
        }

        private long _lastMessageSuccessProcessedDurationInMilliseconds;
        public long? LastMessageSuccessProcessedDurationInMilliseconds
        {
            get
            {
                if (_lastMessageSuccessProcessedDurationInMilliseconds == 0)
                    return null;

                return _lastMessageSuccessProcessedDurationInMilliseconds;
            }
        }

        #endregion

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MessageHandlerInvoker<TMessage, TMessageHandler>> _logger;
        private readonly AsyncPolicy _retryPolicy;

        public MessageHandlerInvoker(IServiceProvider serviceProvider,
            int retryCount,
            TimeSpan retryInterval,
            int maxParallelHandlers)
        {
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _logger = serviceProvider.GetRequiredService<ILogger<MessageHandlerInvoker<TMessage, TMessageHandler>>>();

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt => retryInterval
                    );

            _semaphoreSlim = new SemaphoreSlim(maxParallelHandlers);
        }

        public Delegate CreateHandlerDelegate()
        {
            return (Func<TMessage, Task>)ProcessWithRetryAsync;
        }

        public Type GetHandlerType()
        {
            return typeof(TMessageHandler);
        }

        public async Task ProcessWithRetryAsync(TMessage message)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(_cancellationTokenSource.Token);

                await _retryPolicy.ExecuteAsync(() => ProcessAsync(message));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task ProcessAsync(TMessage message)
        {
            Interlocked.Increment(ref _messagesProcessingAttemptsCount);
            Interlocked.Exchange(ref _lastMessageProcessingAttemptDate, DateTime.Now.ToBinary());

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var handler = ResolveHandler(scope);
                    var watch = new Stopwatch();
                    watch.Start();

                    await handler.ProcessAsync(message);
                    watch.Stop();

                    Interlocked.Increment(ref _messagesSuccessProcessedCount);
                    Interlocked.Exchange(ref _lastMessageSuccessProcessedDate, DateTime.Now.ToBinary());
                    Interlocked.Exchange(ref _lastMessageSuccessProcessedDurationInMilliseconds, watch.ElapsedMilliseconds);
                    Interlocked.Add(ref _sumMessagesProcessingDurationInMilliseconds, watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _messagesErrorProcessedCount);

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

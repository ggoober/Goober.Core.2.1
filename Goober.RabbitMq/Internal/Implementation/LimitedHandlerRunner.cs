using EasyNetQ.Consumer;
using Goober.RabbitMq.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.RabbitMq.Internal.Implementation
{
    class LimitedHandlerRunner : HandlerRunner, IHandlerRunner
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly int _maxParallelHandlers;

        public LimitedHandlerRunner(
            IConsumerErrorStrategy consumerErrorStrategy,
            RabbitMqClientOptions rabbitMqClientOptions)
            : base(consumerErrorStrategy)
        {
            _maxParallelHandlers = 10;// rabbitMqClientOptions.MaxParallelHandlers;
        }

        public override async Task<AckStrategy> InvokeUserMessageHandlerAsync(ConsumerExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var cancellationToken = _cancellationTokenSource.Token;

            var queueSemaphore = _semaphores.GetOrAdd(context.Info.Queue, new SemaphoreSlim(_maxParallelHandlers));

            try
            {
                await queueSemaphore.WaitAsync(_cancellationTokenSource.Token);

                return await base.InvokeUserMessageHandlerAsync(context);
            }
            finally
            {
                queueSemaphore.Release();
            }
        }

        new public void Dispose()
        {
            // free managed resources  

            _cancellationTokenSource.Cancel();

            foreach (var semaphore in _semaphores.Values)
                semaphore.Dispose();

            base.Dispose();
        }
    }
}

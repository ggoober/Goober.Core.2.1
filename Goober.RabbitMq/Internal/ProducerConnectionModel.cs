using EasyNetQ;
using Polly;

namespace Goober.RabbitMq.Internal
{
    class ProducerConnectionModel
    {
        public IBus Bus { get; set; }

        public AsyncPolicy RetryPolicyAsync { get; set; }

        public Policy RetryPolicy { get; set; }

        public uint PublishRetryCount { get; set; } = 10;

        public uint PublishRetryIntervalInMilliseconds { get; set; } = 1000;
    }
}

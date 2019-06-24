using System;
 
namespace Goober.RabbitMq.Models
{
    public class MessageTypeConsumerPingModel
    {
        public string MessageTypeFullName { get; set; }

        public string HandlerTypeFullName { get; set; }

        public long MessagesProcessingAttemptsCount { get; set; }

        public long MessagesSuccessProcessedCount { get; set; }

        public long MessagesErrorProcessedCount { get; set; }

        public DateTime? LastMessageProcessingAttemptDate { get; set; }

        public DateTime? LastMessageSuccessProcessedDate { get; set; }

        public long? AvgMessagesProcessingDurationInMilliseconds { get; set; }

        public long? LastMessageSuccessProcessedDurationInMilliseconds { get; set; }

        public ushort MaxParallelHandlers { get; set; }

        public uint MessageProccessRetryDelayInMilliseconds { get; set; }

        public ushort MessageProcessRetryCount { get; set; }

        public ushort PrefetchCount { get; set; }
    }
}

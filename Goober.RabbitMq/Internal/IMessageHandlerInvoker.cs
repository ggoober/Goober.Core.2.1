using System;

namespace Goober.RabbitMq.Internal
{
    public interface IMessageHandlerInvoker
    {
        Delegate CreateHandlerDelegate();

        Type GetHandlerType();

        long MessagesProcessingAttemptsCount { get; }

        long MessagesSuccessProcessedCount { get; }

        long MessagesErrorProcessedCount { get; }

        DateTime? LastMessageProcessingAttemptDate { get; }

        DateTime? LastMessageSuccessProcessedDate { get; }

        long? AvgMessagesProcessingDurationInMilliseconds { get; }

        long? LastMessageSuccessProcessedDurationInMilliseconds { get; }
    }
}

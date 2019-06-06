using System;

namespace Goober.BackgroundWorker.Models
{
    public class ListBackgroundPingModel
    {
        public int MaxParallelTasks { get; set; }

        public long? LastIterationListItemsCount { get; set; }

        public DateTime? LastIterationListItemExecuteDateTime { get; set; }

        public long? LastIterationListItemsSuccessProcessedCount { get; set; }

        public long? LastIterationListItemsProcessedCount { get; set; }

        public long? LastIterationListItemsAvgDurationInMilliseconds { get; set; }

        public long? LastIterationListItemsLastDurationInMilliseconds { get; set; }
    }
}

using System;

namespace Goober.BackgroundWorker.Models
{
    public class BackgroundWorkerPingModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsRunning { get; set; }

        public long ServiceUpTime { get; set; }

        public long TaskUpTime { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime StopDateTime { get; set; }

        #region iteration

        public long IteratedCount { get; set; }

        public long SuccessIteratedCount { get; set; }

        public DateTime? LastIterationStartDateTime { get; set; }

        public DateTime? LastIterationFinishDateTime { get; set; }

        public long? LastIterationDurationInMilliseconds { get; set; }

        public long? AvgIterationDurationInMilliseconds { get; set; }

        #endregion

        #region list

        public int MaxParallelTasks { get; set; }

        public long? LastIterationListItemsCount { get; set; }

        public DateTime? LastIterationListItemExecuteDateTime { get; set; }

        public long? LastIterationListItemsSuccessProcessedCount { get; set; }

        public long? LastIterationListItemsProcessedCount { get; set; }

        public long? LastIterationListItemsAvgDurationInMilliseconds { get; set; }

        public long? LastIterationListItemsLastDurationInMilliseconds { get; set; }

        #endregion
    }
}

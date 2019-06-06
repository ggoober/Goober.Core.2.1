using System;

namespace Goober.BackgroundWorker.Models
{
    public class IterateBackgroundPingModel
    {
        public long TaskDelayInMilliseconds { get; set; }

        public long IteratedCount { get; set; }

        public long SuccessIteratedCount { get; set; }

        public DateTime? LastIterationStartDateTime { get; set; }

        public DateTime? LastIterationFinishDateTime { get; set; }

        public long? LastIterationDurationInMilliseconds { get; set; }

        public long? AvgIterationDurationInMilliseconds { get; set; }

    }
}

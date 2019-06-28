using System;

namespace Goober.BackgroundWorker.Models
{
    public class BackgroundWorkerPingModel
    {
        public string Name { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsRunning { get; set; }

        public bool IsCancellationRequested { get; set; }

        public long ServiceUpTimeInSec { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? StopDateTime { get; set; }

        public IterateBackgroundPingModel Iterate { get; set; }

        public ListBackgroundPingModel List { get; set; }
    }
}

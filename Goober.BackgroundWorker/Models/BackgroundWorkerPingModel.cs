using System;

namespace Goober.BackgroundWorker.Models
{
    public class BackgroundWorkerPingModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsRunning { get; set; }

        public long ServiceUpTimeInSec { get; set; }

        public long TaskUpTimeInSec { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? StopDateTime { get; set; }

        public IterateBackgroundPingModel Iterate { get; set; }

        public ListBackgroundPingModel List { get; set; }
    }
}

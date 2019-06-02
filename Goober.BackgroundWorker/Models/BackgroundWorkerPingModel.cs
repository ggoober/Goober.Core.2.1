namespace Goober.BackgroundWorker.Models
{
    public class BackgroundWorkerPingModel
    {
        public string Name { get; set; }

        public bool IsRunning { get; set; }

        public long ServiceUpTime { get; set; }

        public long TaskUpTime { get; set; }
    }
}

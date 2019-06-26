namespace Goober.BackgroundWorker.Options
{
    public class BackgroundWorkerOptionModel
    {
        public bool IsDisabled { get; set; } = false;

        public string ClassName { get; set; }

        public uint? IterationDelayInMilliseconds { get; set; }

        public uint? ListMaxDegreeOfParallelism { get; set; }
    }
}

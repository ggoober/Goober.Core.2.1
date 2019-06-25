using System;
using System.Collections.Generic;
using System.Text;

namespace Goober.BackgroundWorker.Models.Metrics
{
    public interface ISimpleBackgroundMetrics
    {
        DateTime? StartDateTime { get; }

        DateTime? StopDateTime { get; }

        bool IsRunning { get; }

        TimeSpan ServiceUpTime { get; }

        TimeSpan TaskUpTime { get; }

        bool IsCancellationRequested { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Goober.BackgroundWorker.Models.Metrics
{
    public interface IIterateBackgroundMetrics
    {
        TimeSpan TaskDelay { get; }

        long IteratedCount { get; }

        long SuccessIteratedCount { get; }

        DateTime? LastIterationStartDateTime { get; }

        DateTime? LastIterationFinishDateTime { get; }

        long? LastIterationDurationInMilliseconds { get; }

        long? AvgIterationDurationInMilliseconds { get; }
    }
}

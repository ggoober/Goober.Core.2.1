using System;
using System.Collections.Generic;
using System.Text;

namespace Goober.BackgroundWorker.BackgroundServices
{
    public interface IIterateBackgroundMetrics
    {
        long IteratedCount { get; }

        long SuccessIteratedCount { get; }

        DateTime? LastIterationStartDateTime { get; }

        DateTime? LastIterationFinishDateTime { get; }

        long? LastIterationDurationInMilliseconds { get; }

        long? AvgIterationDurationInMilliseconds { get; }
    }
}

﻿using System;

namespace Goober.BackgroundWorker.BackgroundServices
{
    public interface IListBackgroundMetrics
    {
        int MaxParallelTasks { get; }

        long? LastIterationListItemsCount { get; }

        DateTime? LastIterationListItemExecuteDateTime { get; }

        long? LastIterationListItemsSuccessProcessedCount { get; }

        long? LastIterationListItemsProcessedCount { get; }

        long? LastIterationListItemsAvgDurationInMilliseconds { get; }

        long? LastIterationListItemsLastDurationInMilliseconds { get; }
    }
}
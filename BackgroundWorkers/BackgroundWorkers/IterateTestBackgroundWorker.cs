using BackgroundWorkers.Services;
using Goober.BackgroundWorker;
using Microsoft.Extensions.Logging;
using System;

namespace BackgroundWorkers.BackgroundWorkers
{
    public class IterateTestBackgroundWorker : IterateBackgroundWorker<IIterateTestBackgroundService>
    {
        public IterateTestBackgroundWorker(ILogger<IterateTestBackgroundWorker> logger, IServiceProvider serviceProvider) 
            : base(logger, serviceProvider)
        {
        }
    }
}

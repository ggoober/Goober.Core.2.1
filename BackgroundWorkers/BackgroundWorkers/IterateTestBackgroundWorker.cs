using BackgroundWorkers.Services;
using Goober.BackgroundWorker;
using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace BackgroundWorkers.BackgroundWorkers
{
    public class IterateTestBackgroundWorker : IterateBackgroundWorker<IIterateTestBackgroundService>
    {
        public IterateTestBackgroundWorker(ILogger<IterateTestBackgroundWorker> logger, IServiceProvider serviceProvider, IOptions<BackgroundWorkersOptions> optionsAccessor) 
            : base(logger, serviceProvider, optionsAccessor)
        {
        }
    }
}

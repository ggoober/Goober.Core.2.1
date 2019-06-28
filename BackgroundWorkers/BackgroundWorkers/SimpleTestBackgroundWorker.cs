using Goober.BackgroundWorker.Options;
using Goober.SimpleBackgroundWorker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundWorkers.BackgroundWorkers
{
    public class SimpleTestBackgroundWorker : SimpleBackgroundWorker
    {
        public SimpleTestBackgroundWorker(ILogger<SimpleTestBackgroundWorker> logger, IServiceProvider serviceProvider, IOptions<BackgroundWorkersOptions> optionsAccessor) 
            : base(logger, serviceProvider, optionsAccessor)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.Sleep(5000);
        }
    }
}

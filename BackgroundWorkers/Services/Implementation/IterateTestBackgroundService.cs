using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundWorkers.Services.Implementation
{
    public class IterateTestBackgroundService: IIterateTestBackgroundService
    {
        private readonly ILogger<IterateTestBackgroundService> _logger;

        public IterateTestBackgroundService(ILogger<IterateTestBackgroundService> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteIterationAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExecuteIterationAsync");
            Thread.Sleep(5000);
            return;
        }
    }
}

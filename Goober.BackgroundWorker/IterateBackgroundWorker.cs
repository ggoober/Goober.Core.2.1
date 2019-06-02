using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Goober.BackgroundWorker
{
    public abstract class IterateBackgroundWorker<TIterateBackgroundService>: BaseBackgroundWorker 
        where TIterateBackgroundService: IIterateBackgroundService
    {
        protected virtual TimeSpan TaskDelay { get; } = TimeSpan.FromMinutes(5);

        public IterateBackgroundWorker(ILogger logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var iterationId = 0;

            while (stoppingToken.IsCancellationRequested == false)
            {
                iterationId++;

                try
                {
                    Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({_id}) iterate ({iterationId}) executing");

                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<TIterateBackgroundService>() as IIterateBackgroundService;
                        if (service == null)
                            throw new InvalidOperationException($"IterateBackgroundWorker.ExecuteAsync ({_id}) iterate ({iterationId}) service {typeof(TIterateBackgroundService).Name}");

                        await service.ExecuteIterationAsync(stoppingToken);
                    }

                    Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({_id}) iterate ({iterationId}) finished");
                }
                catch (Exception exc)
                {
                    this.Logger.LogError(exception: exc, message: $"IterateBackgroundWorker.ExecuteAsync ({_id}) fail iterate ({iterationId})");
                }

                Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({_id}) iterate ({iterationId}) waiting :{TaskDelay.TotalSeconds}s");

                await Task.Delay(TaskDelay, stoppingToken);

                Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({_id}) iterate ({iterationId}) waiting :{TaskDelay.TotalSeconds}s");
            }
        }
    }
}

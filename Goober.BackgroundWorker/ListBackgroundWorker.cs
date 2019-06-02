using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Goober.BackgroundWorker
{
    public class ListBackgroundWorker<TItem, TListBackgroundService> : BaseBackgroundWorker
        where TListBackgroundService : IListBackgroundService<TItem>
    {
        protected virtual TimeSpan TaskDelay { get; } = TimeSpan.FromMinutes(5);

        protected virtual int MaxDegreeOfParallelism { get; } = 1;

        public ListBackgroundWorker(ILogger logger, IServiceProvider serviceProvider) 
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
                    List<TItem> items;

                    Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({_id}) iteration ({iterationId}) executing");

                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                        if (service == null)
                            throw new InvalidOperationException($"ListBackgroundWorker.ExecuteAsync ({_id}) iteration ({iterationId}) service {typeof(TListBackgroundService).Name}");

                        items = await service.GetItemsAsync();
                    }

                    var tasks = new List<Task>();

                    using (var semaphore = new SemaphoreSlim(MaxDegreeOfParallelism))
                    {
                        foreach (var item in items)
                        {
                            await semaphore.WaitAsync();

                            if (stoppingToken.IsCancellationRequested == false)
                            {
                                tasks.Add(
                                    ExecuteItemMethodSafetyAsync(semaphore: semaphore, 
                                        item: item, 
                                        iterationId: iterationId));
                            }
                        }

                        await Task.WhenAll(tasks);
                    }

                    Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({_id}) iteration ({iterationId}) finished");
                }
                catch (Exception exc)
                {
                    this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteAsync ({_id}) iteration ({iterationId}) fail");
                }

                Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({_id}) iteration ({iterationId}) waiting :{TaskDelay.TotalSeconds}s");

                await Task.Delay(TaskDelay, stoppingToken);

                Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({_id}) iteration ({iterationId}) waiting :{TaskDelay.TotalSeconds}s");
            }
        }

        private async Task ExecuteItemMethodSafetyAsync(SemaphoreSlim semaphore, TItem item, int iterationId)
        {
            Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety ({_id}) iteration ({iterationId}) start processing item: {JsonConvert.SerializeObject(item)}");

            try
            {
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                    if (service == null)
                        throw new InvalidOperationException($"ListBackgroundWorker.ExecuteItemMethodSafety ({_id}) iteration ({iterationId}) service {typeof(TListBackgroundService).Name}");


                    await service.ProcessItemAsync(item);
                }

                Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety ({_id}) iteration ({iterationId}) finish processing item: {JsonConvert.SerializeObject(item)}");
            }
            catch (Exception exc)
            {
                this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteItemMethodSafety ({_id}) iteration ({iterationId}) fail, item: {JsonConvert.SerializeObject(item)}");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}

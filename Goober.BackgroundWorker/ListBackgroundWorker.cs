using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Goober.BackgroundWorker.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Goober.BackgroundWorker
{
    public abstract class ListBackgroundWorker<TItem, TListBackgroundService> : BaseBackgroundWorker, IListBackgroundMetrics, IIterateBackgroundMetrics
        where TListBackgroundService : IListBackgroundService<TItem>
    {
        public virtual TimeSpan TaskDelay { get; protected set; } = TimeSpan.FromMinutes(5);

        public int MaxDegreeOfParallelism { get; protected set; } = 1;

        public ListBackgroundWorker(ILogger logger, IServiceProvider serviceProvider) 
            : base(logger, serviceProvider)
        {
        }

        #region fields

        private long _sumIterationsDurationInMilliseconds;

        private  readonly object _iterationListItemUpdateMetricLocker  = new object();

        public long? _lastIterationListItemsSumDurationInMilliseconds;

        #endregion

        #region public properties

        public long IteratedCount { get; private set; }

        public long SuccessIteratedCount { get; private set; }

        public DateTime? LastIterationStartDateTime { get; private set; }

        public DateTime? LastIterationFinishDateTime { get; private set; }

        public long? LastIterationDurationInMilliseconds { get; private set; }

        public long? AvgIterationDurationInMilliseconds { get; private set; }

        public long? LastIterationListItemsCount { get; private set; }

        public DateTime? LastIterationListItemExecuteDateTime { get; private set; }

        public long? LastIterationListItemsSuccessProcessedCount { get; private set; }

        public long? LastIterationListItemsProcessedCount { get; private set; }

        public long? LastIterationListItemsAvgDurationInMilliseconds { get; private set; }

        public long? LastIterationListItemsLastDurationInMilliseconds { get; private set; }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetTaskDelayFromConfiguration();
            SetMaxDegreeOfParallelismFromConfiguration();

            while (stoppingToken.IsCancellationRequested == false)
            {
                IteratedCount++;
                LastIterationStartDateTime = DateTime.Now;

                try
                {
                    var iterationWatch = new Stopwatch(); ;
                    iterationWatch.Start();

                    List<TItem> items;

                    Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({Id}) iteration ({IteratedCount}) executing");

                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                        if (service == null)
                            throw new InvalidOperationException($"ListBackgroundWorker.ExecuteAsync ({Id}) iteration ({IteratedCount}) service {typeof(TListBackgroundService).Name}");

                        items = await service.GetItemsAsync();
                    }

                    LastIterationListItemsCount = items.Count;
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
                                        iterationId: IteratedCount,
                                        stoppingToken: stoppingToken));
                            }
                        }

                        await Task.WhenAll(tasks);
                    }

                    iterationWatch.Stop();
                    LastIterationFinishDateTime = DateTime.Now;
                    SuccessIteratedCount++;
                    LastIterationDurationInMilliseconds = iterationWatch.ElapsedMilliseconds;
                    _sumIterationsDurationInMilliseconds += LastIterationDurationInMilliseconds.Value;
                    AvgIterationDurationInMilliseconds = _sumIterationsDurationInMilliseconds / SuccessIteratedCount;

                    ResetListItemMetrics();

                    Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({Id}) iteration ({IteratedCount}) finished");
                }
                catch (Exception exc)
                {
                    this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteAsync ({Id}) iteration ({IteratedCount}) fail");
                }

                Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({Id}) iteration ({IteratedCount}) waiting :{TaskDelay.TotalSeconds}s");

                LastIterationFinishDateTime = DateTime.Now;

                await Task.Delay(TaskDelay, stoppingToken);

                Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync ({Id}) iteration ({IteratedCount}) waiting :{TaskDelay.TotalSeconds}s");
            }
        }

        private async Task ExecuteItemMethodSafetyAsync(SemaphoreSlim semaphore, TItem item, long iterationId, CancellationToken stoppingToken)
        {
            Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety ({Id}) iteration ({iterationId}) start processing item: {JsonConvert.SerializeObject(item)}");

            lock (_iterationListItemUpdateMetricLocker)
            {
                LastIterationListItemsProcessedCount++;
                LastIterationListItemExecuteDateTime = DateTime.Now;
            }

            try
            {
                var itemWatcher = new Stopwatch();
                itemWatcher.Start();

                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                    if (service == null)
                        throw new InvalidOperationException($"ListBackgroundWorker.ExecuteItemMethodSafety ({Id}) iteration ({iterationId}) service {typeof(TListBackgroundService).Name}");


                    await service.ProcessItemAsync(item, stoppingToken);
                }

                itemWatcher.Stop();

                lock (_iterationListItemUpdateMetricLocker)
                {
                    LastIterationListItemsSuccessProcessedCount++;
                    LastIterationListItemsLastDurationInMilliseconds = itemWatcher.ElapsedMilliseconds;
                    _lastIterationListItemsSumDurationInMilliseconds += LastIterationListItemsLastDurationInMilliseconds;
                    LastIterationListItemsAvgDurationInMilliseconds = _lastIterationListItemsSumDurationInMilliseconds / LastIterationListItemsSuccessProcessedCount;
                }

                Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety ({Id}) iteration ({iterationId}) finish processing item: {JsonConvert.SerializeObject(item)}");
            }
            catch (Exception exc)
            {
                this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteItemMethodSafety ({Id}) iteration ({iterationId}) fail, item: {JsonConvert.SerializeObject(item)}");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            ResetMetrics();

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            ResetMetrics();

            return base.StopAsync(cancellationToken);
        }

        private void ResetMetrics()
        {
            IteratedCount = 0;
            SuccessIteratedCount = 0;

            LastIterationStartDateTime = null;
            LastIterationFinishDateTime = null;

            LastIterationDurationInMilliseconds = 0;
            _sumIterationsDurationInMilliseconds = 0;
            ResetListItemMetrics();
        }

        private void ResetListItemMetrics()
        {
            LastIterationListItemsCount = 0;
            LastIterationListItemsProcessedCount = 0;
            LastIterationListItemsSuccessProcessedCount = 0;

            _lastIterationListItemsSumDurationInMilliseconds = 0;
            LastIterationListItemsLastDurationInMilliseconds = 0;
            LastIterationListItemsAvgDurationInMilliseconds = 0;
            LastIterationListItemExecuteDateTime = null;
        }

        private void SetTaskDelayFromConfiguration()
        {
            var configuration = ServiceProvider.GetService<IConfiguration>();
            var taskDelayInMillisecondsConfigKey = this.GetType().Name + ".TaskDelayInMilliseconds";
            var taskDelayInMilliseconds = ToInt(configuration[taskDelayInMillisecondsConfigKey]);
            if (taskDelayInMilliseconds.HasValue)
            {
                TaskDelay = TimeSpan.FromMilliseconds(taskDelayInMilliseconds.Value);
            }
        }

        private void SetMaxDegreeOfParallelismFromConfiguration()
        {
            var configuration = ServiceProvider.GetService<IConfiguration>();
            var maxDegreeOfParallelismConfigKey = this.GetType().Name + ".MaxDegreeOfParallelism";
            var maxDegreeOfParallelism = ToInt(configuration[maxDegreeOfParallelismConfigKey]);
            if (maxDegreeOfParallelism.HasValue)
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism.Value;
            }
        }


        public static int? ToInt(string value)
        {
            float ret;
            if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out ret))
                return Convert.ToInt32(ret);

            return null;
        }
    }
}

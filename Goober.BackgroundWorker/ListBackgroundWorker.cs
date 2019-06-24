using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Goober.BackgroundWorker.Models.Metrics;
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

        private long _lastIterationListItemsSumDurationInMilliseconds;

        #endregion

        #region public properties

        public long IteratedCount { get; private set; }

        public long SuccessIteratedCount { get; private set; }

        public DateTime? LastIterationStartDateTime { get; private set; }

        public DateTime? LastIterationFinishDateTime { get; private set; }

        public long? LastIterationDurationInMilliseconds { get; private set; }

        public long? AvgIterationDurationInMilliseconds { get; private set; }

        public long? LastIterationListItemsCount { get; private set; }


        private long _lastIterationListItemExecuteDateTimeInBinnary;
        public DateTime? LastIterationListItemExecuteDateTime
        {
            get
            {
                if (_lastIterationListItemExecuteDateTimeInBinnary == 0)
                    return null;

                return DateTime.FromBinary(_lastIterationListItemExecuteDateTimeInBinnary);
            }
        }


        private long _lastIterationListItemsSuccessProcessedCount;
        public long LastIterationListItemsSuccessProcessedCount => _lastIterationListItemsSuccessProcessedCount;


        private long _lastIterationListItemsProcessedCount;
        public long LastIterationListItemsProcessedCount => _lastIterationListItemsProcessedCount;


        public long LastIterationListItemsAvgDurationInMilliseconds
        {
            get
            {
                if (_lastIterationListItemsSuccessProcessedCount == 0)
                    return 0;

                return _lastIterationListItemsSumDurationInMilliseconds / _lastIterationListItemsSuccessProcessedCount;
            }
        }


        private long _lastIterationListItemsLastDurationInMilliseconds;
        public long LastIterationListItemsLastDurationInMilliseconds => _lastIterationListItemsLastDurationInMilliseconds;

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

                    Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} ({Id}) iteration ({IteratedCount}) executing");

                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                        if (service == null)
                            throw new InvalidOperationException($"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} ({Id}) iteration ({IteratedCount}) service {typeof(TListBackgroundService).Name}");

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

                    Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} ({Id}) iteration ({IteratedCount}) finished");
                }
                catch (Exception exc)
                {
                    this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} ({Id}) iteration ({IteratedCount}) fail");
                }

                Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} ({Id}) iteration ({IteratedCount}) waiting :{TaskDelay.TotalSeconds}s");

                LastIterationFinishDateTime = DateTime.Now;

                await Task.Delay(TaskDelay, stoppingToken);

                Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} ({Id}) iteration ({IteratedCount}) waiting :{TaskDelay.TotalSeconds}s");
            }
        }

        private async Task ExecuteItemMethodSafetyAsync(SemaphoreSlim semaphore, TItem item, long iterationId, CancellationToken stoppingToken)
        {
            Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} ({Id}) iteration ({iterationId}) start processing item: {JsonConvert.SerializeObject(item)}");

            Interlocked.Increment(ref _lastIterationListItemsProcessedCount);
            Interlocked.Exchange(ref _lastIterationListItemExecuteDateTimeInBinnary, DateTime.Now.ToBinary());

            try
            {
                var itemWatcher = new Stopwatch();
                itemWatcher.Start();

                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                    if (service == null)
                        throw new InvalidOperationException($"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} ({Id}) iteration ({iterationId}) service {typeof(TListBackgroundService).Name}");


                    await service.ProcessItemAsync(item, stoppingToken);
                }

                itemWatcher.Stop();

                Interlocked.Increment(ref _lastIterationListItemsSuccessProcessedCount);
                Interlocked.Exchange(ref _lastIterationListItemsLastDurationInMilliseconds, itemWatcher.ElapsedMilliseconds);
                Interlocked.Add(ref _lastIterationListItemsSumDurationInMilliseconds, _lastIterationListItemsLastDurationInMilliseconds);

                Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} ({Id}) iteration ({iterationId}) finish processing item: {JsonConvert.SerializeObject(item)}");
            }
            catch (Exception exc)
            {
                this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} ({Id}) iteration ({iterationId}) fail, item: {JsonConvert.SerializeObject(item)}");
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
            AvgIterationDurationInMilliseconds = 0;

            ResetListItemMetrics();
        }

        private void ResetListItemMetrics()
        {
            LastIterationListItemsCount = 0;
            _lastIterationListItemsProcessedCount = 0;
            _lastIterationListItemsSuccessProcessedCount = 0;

            _lastIterationListItemsSumDurationInMilliseconds = 0;
            _lastIterationListItemsLastDurationInMilliseconds = 0;
            _lastIterationListItemExecuteDateTimeInBinnary = 0;
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Goober.BackgroundWorker.Extensions;
using Goober.BackgroundWorker.Models.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Goober.BackgroundWorker
{
    public abstract class ListBackgroundWorker<TItem, TListBackgroundService> : BaseBackgroundWorker, IHostedService, IListBackgroundMetrics, IIterateBackgroundMetrics
        where TListBackgroundService : IListBackgroundService<TItem>
    {
        #region fields

        private long _sumIterationsDurationInMilliseconds;

        private long _lastIterationListItemsSumDurationInMilliseconds;

        #endregion

        #region ctor

        public ListBackgroundWorker(ILogger logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        #endregion

        #region public properties IIterateBackgroundMetrics

        public virtual TimeSpan TaskDelay { get; protected set; } = TimeSpan.FromMinutes(5);

        public long IteratedCount { get; private set; }

        public long SuccessIteratedCount { get; private set; }

        public DateTime? LastIterationStartDateTime { get; private set; }

        public DateTime? LastIterationFinishDateTime { get; private set; }

        public long? LastIterationDurationInMilliseconds { get; private set; }

        public long? AvgIterationDurationInMilliseconds { get; private set; }

        #endregion

        #region public properties IListBackgroundMetrics

        public int MaxDegreeOfParallelism { get; protected set; } = 1;

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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SetTaskDelayFromConfiguration();
            SetMaxDegreeOfParallelismFromConfiguration();

            Action<Task> repeatAction = null;
            repeatAction = _ignored1 =>
            {
                IteratedCount++;
                LastIterationStartDateTime = DateTime.Now;

                var iterationWatch = new Stopwatch(); ;
                iterationWatch.Start();

                try
                {
                    Logger.LogInformation($"Start executing iteration ListBackgroundWorker.ExecuteIteration {this.GetType().Name} iteration ({IteratedCount})");

                    var items = GetItemsListAsync();
                    LastIterationListItemsCount = items.Count;

                    ProcessListItemsAsync(items).Wait();

                    iterationWatch.Stop();
                    LastIterationFinishDateTime = DateTime.Now;
                    SuccessIteratedCount++;
                    LastIterationDurationInMilliseconds = iterationWatch.ElapsedMilliseconds;
                    _sumIterationsDurationInMilliseconds += LastIterationDurationInMilliseconds.Value;
                    AvgIterationDurationInMilliseconds = _sumIterationsDurationInMilliseconds / SuccessIteratedCount;

                    ResetListMetrics();

                    Logger.LogInformation($"Finish iteration ListBackgroundWorker.ExecuteIteration {this.GetType().Name} iteration ({IteratedCount})");
                }
                catch (Exception exc)
                {
                    Logger.LogError(message: $"Error ListBackgroundWorker.ExecuteIteration {this.GetType().Name} iteration ({IteratedCount})",
                        exception: exc);
                }

                Task.Delay(TaskDelay, StoppingCts.Token)
                    .ContinueWith(_ignored2 => repeatAction(_ignored2), StoppingCts.Token);
            };

            Task.Delay(5000, StoppingCts.Token).ContinueWith(continuationAction: repeatAction, cancellationToken: StoppingCts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                SetWorkerIsStopping();
            }
            finally
            {
                Logger.LogInformation($"ListBackgroundWorker {this.GetType().Name} stopped");
                SetWorkerHasStopped();
            }

            return Task.CompletedTask;
        }

        private List<TItem> GetItemsListAsync()
        {
            Logger.LogInformation($"ListBackgroundWorker.ExecuteAsync {this.GetType().Name} iteration ({IteratedCount}) executing");

            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TListBackgroundService>() as IListBackgroundService<TItem>;
                if (service == null)
                    throw new InvalidOperationException($"Error resolve service {typeof(TListBackgroundService).Name} ListBackgroundWorker.ExecuteAsync {this.GetType().Name} iteration ({IteratedCount})");

                var task = Task.Run(() => service.GetItemsAsync());

                task.Wait();

                return task.Result;
            }
        }

        private void ExecuteItemMethodSafety(SemaphoreSlim semaphore, TItem item, long iterationId, CancellationToken stoppingToken)
        {
            Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} iteration ({iterationId}) start processing item: {JsonConvert.SerializeObject(item)}");

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
                        throw new InvalidOperationException($"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} iteration ({iterationId}) service {typeof(TListBackgroundService).Name}");


                    var processTask = Task.Run(() => service.ProcessItemAsync(item, stoppingToken));
                    processTask.Wait();
                }

                itemWatcher.Stop();

                Interlocked.Increment(ref _lastIterationListItemsSuccessProcessedCount);
                Interlocked.Exchange(ref _lastIterationListItemsLastDurationInMilliseconds, itemWatcher.ElapsedMilliseconds);
                Interlocked.Add(ref _lastIterationListItemsSumDurationInMilliseconds, _lastIterationListItemsLastDurationInMilliseconds);

                Logger.LogInformation($"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} iteration ({iterationId}) finish processing item: {JsonConvert.SerializeObject(item)}");
            }
            catch (Exception exc)
            {
                this.Logger.LogError(exception: exc, message: $"ListBackgroundWorker.ExecuteItemMethodSafety {this.GetType().Name} iteration ({iterationId}) fail, item: {JsonConvert.SerializeObject(item)}");
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task ProcessListItemsAsync(List<TItem> items)
        {
            var tasks = new List<Task>();

            using (var semaphore = new SemaphoreSlim(MaxDegreeOfParallelism))
            {
                foreach (var item in items)
                {
                    await semaphore.WaitAsync();

                    if (StoppingCts.IsCancellationRequested == true)
                    {
                        break;
                    }

                    tasks.Add(Task.Run(() =>
                            ExecuteItemMethodSafety(semaphore: semaphore,
                                item: item,
                                iterationId: IteratedCount,
                                stoppingToken: StoppingCts.Token)));
                }

                await Task.WhenAll(tasks);
            }
        }

        protected override void SetWorkerHasStopped()
        {
            IteratedCount = 0;
            SuccessIteratedCount = 0;

            LastIterationStartDateTime = null;
            LastIterationFinishDateTime = null;

            LastIterationDurationInMilliseconds = 0;
            _sumIterationsDurationInMilliseconds = 0;
            AvgIterationDurationInMilliseconds = 0;

            ResetListMetrics();

            base.SetWorkerHasStopped();
        }

        private void ResetListMetrics()
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
            var taskDelayInMilliseconds = configuration[taskDelayInMillisecondsConfigKey].ToInt();
            if (taskDelayInMilliseconds.HasValue)
            {
                TaskDelay = TimeSpan.FromMilliseconds(taskDelayInMilliseconds.Value);
            }
        }

        private void SetMaxDegreeOfParallelismFromConfiguration()
        {
            var configuration = ServiceProvider.GetService<IConfiguration>();
            var maxDegreeOfParallelismConfigKey = this.GetType().Name + ".MaxDegreeOfParallelism";
            var maxDegreeOfParallelism = configuration[maxDegreeOfParallelismConfigKey].ToInt();
            if (maxDegreeOfParallelism.HasValue)
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism.Value;
            }
        }
    }
}

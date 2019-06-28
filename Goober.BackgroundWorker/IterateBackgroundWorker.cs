using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Goober.BackgroundWorker.Extensions;
using Goober.BackgroundWorker.Models.Metrics;
using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Goober.BackgroundWorker
{
    public abstract class IterateBackgroundWorker<TIterateBackgroundService> : BaseBackgroundWorker, IHostedService, IIterateBackgroundMetrics
        where TIterateBackgroundService : IIterateBackgroundService
    {
        #region fields


        private long _sumIterationsDurationInMilliseconds { get; set; }

        #endregion

        #region public properties IIterateBackgroundMetrics

        public TimeSpan TaskDelay { get; protected set; }

        public long IteratedCount { get; protected set; }

        public long SuccessIteratedCount { get; protected set; }

        public DateTime? LastIterationStartDateTime { get; protected set; }

        public DateTime? LastIterationFinishDateTime { get; protected set; }

        public long? LastIterationDurationInMilliseconds { get; protected set; }

        public long? AvgIterationDurationInMilliseconds { get; protected set; }

        #endregion

        #region ctor

        public IterateBackgroundWorker(ILogger logger, IServiceProvider serviceProvider, IOptions<BackgroundWorkersOptions> optionsAccessor)
            : base(logger, serviceProvider, optionsAccessor)
        {
            var iterationDelayInMilliseconds = _options.IterationDelayInMilliseconds ?? 900000;
            TaskDelay = TimeSpan.FromMilliseconds(iterationDelayInMilliseconds);
        }

        #endregion

        #region IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsDisabled == true)
            {
                Logger.LogInformation(message: $"{ClassName} is disabled");
                return Task.CompletedTask;
            }

            SetWorkerIsStarting();
            Action<Task> repeatAction = null;
            repeatAction = _ignored1 =>
                {
                    try
                    {
                        ExecuteIteration();
                    }
                    catch (Exception exc)
                    {
                        Logger.LogError(exception: exc,
                            message: $"Error executeIteration for worker {ClassName} iterate ({IteratedCount})");
                    }

                    Task.Delay(TaskDelay, StoppingCts.Token)
                        .ContinueWith(_ignored2 => repeatAction(_ignored2), StoppingCts.Token);
                };

            Task.Delay(5000, StoppingCts.Token).ContinueWith(continuationAction: repeatAction, cancellationToken: StoppingCts.Token);

            SetWorkerHasStarted();

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
                Logger.LogInformation($"Worker {this.GetType().Name} stopped");
                SetWorkerHasStopped();
            }

            return Task.CompletedTask;
        }

        #endregion

        private void ExecuteIteration()
        {
            IteratedCount++;
            var iterationWatch = new Stopwatch();
            iterationWatch.Start();
            LastIterationStartDateTime = DateTime.Now;

            Logger.LogInformation($"Worker {ClassName} iterate ({IteratedCount}) executing");

            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TIterateBackgroundService>() as IIterateBackgroundService;
                if (service == null)
                    throw new InvalidOperationException($"Can't resolve service {typeof(TIterateBackgroundService).Name} for worker {ClassName} iterate ({IteratedCount})");

                var executeTask = Task.Run(() => service.ExecuteIterationAsync(StoppingCts.Token));

                executeTask.Wait();
            }

            iterationWatch.Stop();
            SuccessIteratedCount++;
            LastIterationFinishDateTime = DateTime.Now;
            LastIterationDurationInMilliseconds = iterationWatch.ElapsedMilliseconds;
            _sumIterationsDurationInMilliseconds += LastIterationDurationInMilliseconds.Value;
            AvgIterationDurationInMilliseconds = _sumIterationsDurationInMilliseconds / SuccessIteratedCount;

            Logger.LogInformation($"Worker {ClassName} iterate ({IteratedCount}) finished");
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
            _sumIterationsDurationInMilliseconds = 0;

            base.SetWorkerHasStopped();
        }
    }
}

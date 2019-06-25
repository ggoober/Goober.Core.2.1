using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Goober.BackgroundWorker.Extensions;
using Goober.BackgroundWorker.Models.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Goober.BackgroundWorker
{
    public abstract class IterateBackgroundWorker<TIterateBackgroundService>: BaseBackgroundWorker, IHostedService, IIterateBackgroundMetrics
        where TIterateBackgroundService: IIterateBackgroundService
    {
        #region fields

        private Action<Task> _repeatAction;

        private long _sumIterationsDurationInMilliseconds { get; set; }

        #endregion

        #region public properties IIterateBackgroundMetrics

        public TimeSpan TaskDelay { get; protected set; } = TimeSpan.FromMilliseconds(15);

        public long IteratedCount { get; protected set; }

        public long SuccessIteratedCount { get; protected set; }

        public DateTime? LastIterationStartDateTime { get; protected set; }

        public DateTime? LastIterationFinishDateTime { get; protected set; }

        public long? LastIterationDurationInMilliseconds { get; protected set; }

        public long? AvgIterationDurationInMilliseconds { get; protected set; }

        #endregion

        #region ctor

        public IterateBackgroundWorker(ILogger logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
            
        }

        #endregion

        #region IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            SetTaskDelayFromConfiguration();

            SetWorkerIsStarting();

            _repeatAction = _ignored1 =>
            {
                try
                {
                    ExecuteIteration();
                }
                catch (Exception exc)
                {
                    Logger.LogError(exception: exc,
                        message: $"Fail IterateBackgroundWorker.ExecuteIteration {this.GetType().Name} iterate ({IteratedCount})");
                }

                Task.Delay(TaskDelay, StoppingCts.Token)
                    .ContinueWith(_ignored2 => _repeatAction(_ignored2), StoppingCts.Token);
            };

            return Task.Delay(5000, StoppingCts.Token).ContinueWith(continuationAction: _repeatAction, cancellationToken: StoppingCts.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            try
            {
                SetWorkerIsStopping();
            }
            finally
            {
                Logger.LogInformation($"SimpleBackgroundWorker {this.GetType().Name} stopped");
                SetWorkerHasStopped();
            }

            return Task.CompletedTask;
        }

        #endregion

        private void ExecuteIteration()
        {
            var iterationWatch = new Stopwatch();
            iterationWatch.Start();

            Logger.LogInformation($"IterateBackgroundWorker.ExecuteIteration {this.GetType().Name} iterate ({IteratedCount}) executing");

            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<TIterateBackgroundService>() as IIterateBackgroundService;
                if (service == null)
                    throw new InvalidOperationException($"IterateBackgroundWorker.ExecuteIteration {this.GetType().Name} iterate ({IteratedCount}) service {typeof(TIterateBackgroundService).Name}");

                var executeTask = service.ExecuteIterationAsync(StoppingCts.Token);

                executeTask.Wait();
            }

            iterationWatch.Stop();
            SuccessIteratedCount++;
            LastIterationDurationInMilliseconds = iterationWatch.ElapsedMilliseconds;
            _sumIterationsDurationInMilliseconds += LastIterationDurationInMilliseconds.Value;
            AvgIterationDurationInMilliseconds = _sumIterationsDurationInMilliseconds / SuccessIteratedCount;

            Logger.LogInformation($"IterateBackgroundWorker.ExecuteIteration {this.GetType().Name} iterate ({IteratedCount}) finished");
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

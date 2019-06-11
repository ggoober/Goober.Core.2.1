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

namespace Goober.BackgroundWorker
{
    public abstract class IterateBackgroundWorker<TIterateBackgroundService>: BaseBackgroundWorker, IIterateBackgroundMetrics
        where TIterateBackgroundService: IIterateBackgroundService
    {
        public virtual TimeSpan TaskDelay { get; protected set; } = TimeSpan.FromMinutes(5);

        public IterateBackgroundWorker(ILogger logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        #region fields

        private long _sumIterationsDurationInMilliseconds { get; set; }

        #endregion

        #region public properties

        public long IteratedCount { get; protected set; }

        public long SuccessIteratedCount { get; protected set; }

        public DateTime? LastIterationStartDateTime { get; protected set; }

        public DateTime? LastIterationFinishDateTime { get; protected set; }

        public long? LastIterationDurationInMilliseconds { get; protected set; }

        public long? AvgIterationDurationInMilliseconds { get; protected set; }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetTaskDelayFromConfiguration();

            while (stoppingToken.IsCancellationRequested == false)
            {
                IteratedCount++;
                LastIterationStartDateTime = DateTime.Now;

                try
                {
                    var iterationWatch = new Stopwatch();
                    iterationWatch.Start();

                    Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({Id}) iterate ({IteratedCount}) executing");

                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetRequiredService<TIterateBackgroundService>() as IIterateBackgroundService;
                        if (service == null)
                            throw new InvalidOperationException($"IterateBackgroundWorker.ExecuteAsync ({Id}) iterate ({IteratedCount}) service {typeof(TIterateBackgroundService).Name}");

                        await service.ExecuteIterationAsync(stoppingToken);

                    }

                    iterationWatch.Stop();
                    SuccessIteratedCount++;
                    LastIterationDurationInMilliseconds = iterationWatch.ElapsedMilliseconds;
                    _sumIterationsDurationInMilliseconds += LastIterationDurationInMilliseconds.Value;
                    AvgIterationDurationInMilliseconds = _sumIterationsDurationInMilliseconds / SuccessIteratedCount;

                    Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({Id}) iterate ({IteratedCount}) finished");
                }
                catch (Exception exc)
                {
                    this.Logger.LogError(exception: exc, message: $"IterateBackgroundWorker.ExecuteAsync ({Id}) fail iterate ({IteratedCount})");
                }

                LastIterationFinishDateTime = DateTime.Now;

                Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({Id}) iterate ({IteratedCount}) waiting :{TaskDelay.TotalSeconds}s");

                await Task.Delay(TaskDelay, stoppingToken);

                Logger.LogInformation($"IterateBackgroundWorker.ExecuteAsync ({Id}) iterate ({IteratedCount}) waiting :{TaskDelay.TotalSeconds}s");
            }
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

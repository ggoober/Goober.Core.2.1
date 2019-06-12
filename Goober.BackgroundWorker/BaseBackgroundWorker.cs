using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.BackgroundWorker
{
    public abstract class BaseBackgroundWorker : IHostedService
    {
        #region fields

        private Task _executingTask;

        private Task _stoppingTask;

        private CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private Stopwatch _serviceWatch = new Stopwatch();
        private Stopwatch _taskWatch = new Stopwatch();

        #endregion

        #region protected properties

        protected IServiceProvider ServiceProvider { get; private set; }

        protected IServiceScopeFactory ServiceScopeFactory { get; private set; }

        protected ILogger Logger { get; private set; }

        #endregion

        #region public properties

        public string Id { get; protected set; } = "none";

        public DateTime? StartDateTime { get; protected set; }

        public DateTime? StopDateTime { get; protected set; }

        public bool IsRunning { get; protected set; } = false;

        public TimeSpan ServiceUpTime => _serviceWatch.Elapsed;

        public TimeSpan TaskUpTime => _taskWatch.Elapsed;

        public bool IsCancellationRequested => _stoppingCts?.IsCancellationRequested ?? false;

        #endregion

        #region ctor

        public BaseBackgroundWorker(ILogger logger, IServiceProvider serviceProvider)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        #endregion

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsRunning == true)
            {
                throw new InvalidOperationException($"BackgroundWorker ({Id}) start failed, task already executing...");
            }

            Logger.LogInformation($"BackgroundWorker {this.GetType().Name} is starting...");

            StartDateTime = DateTime.Now;
            _serviceWatch.Start();
            _taskWatch.Start();

            try
            {

                _executingTask = ExecuteAsync(_stoppingCts.Token);
                _stoppingTask = _executingTask.ContinueWith(FinalizeMetrics);

                Id = _executingTask.Id.ToString();
                IsRunning = true;

                Logger.LogInformation($"BackgroundWorker {this.GetType().Name} ({Id}) has started.");
            }
            catch (Exception exc)
            {
                Logger.LogCritical(exc, $"BackgroundWorker {this.GetType().Name} ({Id}) start fail");

                SetMetricsOnStop();
            }

            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"BackgroundWorker {this.GetType().Name} ({Id}) stoping...");
            StartDateTime = null;
            StopDateTime = DateTime.Now;

            try
            {
                _stoppingCts.Cancel();


                if (IsRunning == true)
                {
                    var configuration = ServiceProvider.GetService<IConfiguration>();
                    var stoppingTimeoutMillisecondsKey = this.GetType().Name + ".StoppingTimeoutMilliseconds";

                    var stoppingTimeoutMilliseconds = ToInt(configuration[stoppingTimeoutMillisecondsKey]) ?? 5000;

                    _stoppingTask.Wait(stoppingTimeoutMilliseconds);
                }
            }
            finally
            {
                SetMetricsOnStop();
                Logger.LogInformation($"BackgroundWorker {this.GetType().Name} ({Id}) stopped");
            }

            return _stoppingTask.IsCompleted ? _stoppingTask : Task.CompletedTask;
        }

        private void FinalizeMetrics(Task t)
        {
            SetMetricsOnStop();

            Logger.LogInformation($"BackgroundWorker ({Id}) finalized");
        }

        private void SetMetricsOnStop()
        {
            IsRunning = false;
            _taskWatch.Reset();
            Id = "none";
            _stoppingCts = new CancellationTokenSource();
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        protected static int? ToInt(string value)
        {
            float ret;
            if (float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out ret))
                return Convert.ToInt32(ret);

            return null;
        }
    }
}

using Goober.BackgroundWorker.Models.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace Goober.BackgroundWorker
{
    public abstract class BaseBackgroundWorker : ISimpleBackgroundMetrics
    {
        #region fields

        private Stopwatch _serviceWatch = new Stopwatch();
        private Stopwatch _taskWatch = new Stopwatch();

        #endregion

        #region protected properties

        protected CancellationTokenSource StoppingCts = new CancellationTokenSource();

        protected IServiceProvider ServiceProvider { get; private set; }

        protected IServiceScopeFactory ServiceScopeFactory { get; private set; }

        protected ILogger Logger { get; private set; }

        #endregion

        #region public properties ISimpleBackgroundMetrics

        public DateTime? StartDateTime { get; protected set; }

        public DateTime? StopDateTime { get; protected set; }

        public bool IsRunning { get; protected set; } = false;

        public TimeSpan ServiceUpTime => _serviceWatch.Elapsed;

        public TimeSpan TaskUpTime => _taskWatch.Elapsed;

        public bool IsCancellationRequested => StoppingCts?.IsCancellationRequested ?? false;

        #endregion

        #region ctor

        public BaseBackgroundWorker(ILogger logger, IServiceProvider serviceProvider)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        #endregion

        #region protected methods

        protected virtual void SetWorkerIsStarting()
        {
            if (IsRunning == true)
            {
                throw new InvalidOperationException($"BackgroundWorker {this.GetType().Name} start failed, task already executing...");
            }

            Logger.LogInformation($"BackgroundWorker {this.GetType().Name} is starting...");

            StartDateTime = DateTime.Now;
            _serviceWatch.Start();
            _taskWatch.Start();
        }

        protected virtual void SetWorkerHasStarted()
        {
            IsRunning = true;

            Logger.LogInformation($"BackgroundWorker {this.GetType().Name} has started.");
        }

        protected virtual void SetWorkerIsStopping()
        {
            Logger.LogInformation($"SimpleBackgroundWorker {this.GetType().Name} stoping...");
            StoppingCts.Cancel();
        }

        protected virtual void SetWorkerHasStopped()
        {
            IsRunning = false;
            StartDateTime = null;
            StopDateTime = DateTime.Now;
            StoppingCts = new CancellationTokenSource();

            Logger.LogInformation($"SimpleBackgroundWorker {this.GetType().Name} has stopped");
        }

        #endregion
    }
}

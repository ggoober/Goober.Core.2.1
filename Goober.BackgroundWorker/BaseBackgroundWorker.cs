using Goober.BackgroundWorker.Models.Metrics;
using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Goober.BackgroundWorker
{
    public abstract class BaseBackgroundWorker : ISimpleBackgroundMetrics
    {
        #region fields

        private Stopwatch _serviceWatch = new Stopwatch();

        #endregion

        #region protected properties

        protected string ClassName { get; set; }

        protected BackgroundWorkerOptionModel _options;

        protected CancellationTokenSource StoppingCts = new CancellationTokenSource();

        protected IServiceProvider ServiceProvider { get; private set; }

        protected IServiceScopeFactory ServiceScopeFactory { get; private set; }

        protected ILogger Logger { get; private set; }

        #endregion

        #region public properties ISimpleBackgroundMetrics

        public bool IsDisabled { get; set; }

        public DateTime? StartDateTime { get; protected set; }

        public DateTime? StopDateTime { get; protected set; }

        public bool IsRunning { get; protected set; } = false;

        public TimeSpan ServiceUpTime => _serviceWatch.Elapsed;

        public bool IsCancellationRequested => StoppingCts?.IsCancellationRequested ?? false;

        #endregion

        #region ctor

        public BaseBackgroundWorker(ILogger logger, IServiceProvider serviceProvider,
            IOptions<BackgroundWorkersOptions> optionsAccessor)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            ClassName = this.GetType().Name;

            _options = optionsAccessor.Value?.Workers?.FirstOrDefault(x => x.ClassName == ClassName);
            if (_options == null)
            {
                _options = new BackgroundWorkerOptionModel();
            }

            IsDisabled = _options.IsDisabled;
        }

        #endregion

        #region protected methods

        protected virtual void SetWorkerIsStarting()
        {
            if (IsRunning == true)
            {
                throw new InvalidOperationException($"Worker {ClassName} start failed, task already executing...");
            }

            Logger.LogInformation($"Worker {ClassName} is starting...");
        }

        protected virtual void SetWorkerHasStarted()
        {
            IsRunning = true;

            Logger.LogInformation($"Worker {ClassName} has started.");

            StartDateTime = DateTime.Now;
            _serviceWatch.Start();
        }

        protected virtual void SetWorkerIsStopping()
        {
            Logger.LogInformation($"Worker {ClassName} stoping...");
            StoppingCts.Cancel();
        }

        protected virtual void SetWorkerHasStopped()
        {
            IsRunning = false;
            StartDateTime = null;
            StopDateTime = DateTime.Now;
            StoppingCts = new CancellationTokenSource();

            Logger.LogInformation($"Worker {ClassName} has stopped");
        }

        #endregion
    }
}

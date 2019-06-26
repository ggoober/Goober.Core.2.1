using Goober.BackgroundWorker;
using Goober.BackgroundWorker.Extensions;
using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.SimpleBackgroundWorker
{
    public abstract class SimpleSimpleBackgroundWorker : BaseBackgroundWorker, IHostedService
    {
        #region fields

        private Task _executingTask;

        private Task _stoppingTask;

        #endregion

        #region ctor

        public SimpleSimpleBackgroundWorker(ILogger logger, IServiceProvider serviceProvider, IOptions<BackgroundWorkersOptions> optionsAccessor)
            : base(logger, serviceProvider, optionsAccessor)
        {
        }

        #endregion

        #region IHostedService

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsDisabled == true)
            {
                Logger.LogInformation(message: $"SimpleBackgroundWorker {this.GetType().Name} is disabled");
                return Task.CompletedTask;
            }

            SetWorkerIsStarting();

            try
            {
                _executingTask = ExecuteAsync(StoppingCts.Token);
                _stoppingTask = _executingTask.ContinueWith(FinalizeMetrics);

                SetWorkerHasStarted();
            }
            catch (Exception exc)
            {
                Logger.LogCritical(exc, $"SimpleBackgroundWorker {this.GetType().Name} start fail");

                SetWorkerHasStopped();
            }

            return Task.CompletedTask;
        }

        #region private methods

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            SetWorkerIsStopping();

            try
            {
                if (IsRunning == true)
                {
                    var configuration = ServiceProvider.GetService<IConfiguration>();
                    var stoppingTimeoutMillisecondsKey = this.GetType().Name + ".StoppingTimeoutMilliseconds";

                    var stoppingTimeoutMilliseconds = configuration[stoppingTimeoutMillisecondsKey].ToInt() ?? 5000;

                    _stoppingTask.Wait(stoppingTimeoutMilliseconds);
                }
            }
            finally
            {
                Logger.LogInformation($"SimpleBackgroundWorker {this.GetType().Name} stopped");
                SetWorkerHasStopped();
            }

            return _stoppingTask.IsCompleted ? _stoppingTask : Task.CompletedTask;
        }

        #endregion

        #endregion

        private void FinalizeMetrics(Task task)
        {
            if (task.Status == TaskStatus.Faulted)
            {
                if (task.Exception != null)
                {
                    Logger.LogError(exception: task.Exception, message: $"SimpleBackgroundWorker fault");
                }
                else
                {
                    Logger.LogError(message: $"SimpleBackgroundWorker fault without exception");
                }
            }

            SetWorkerHasStopped();
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}

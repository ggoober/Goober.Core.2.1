using Goober.BackgroundWorker;
using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.SimpleBackgroundWorker
{
    public abstract class SimpleBackgroundWorker : BaseBackgroundWorker, IHostedService
    {
        #region ctor

        public SimpleBackgroundWorker(ILogger logger, IServiceProvider serviceProvider, IOptions<BackgroundWorkersOptions> optionsAccessor)
            : base(logger, serviceProvider, optionsAccessor)
        {
        }

        #endregion

        #region IHostedService

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsDisabled == true)
            {
                Logger.LogInformation(message: $"{this.GetType().Name} is disabled");
                return Task.CompletedTask;
            }

            SetWorkerIsStarting();

            try
            {
                var executingTask = new Task(() => ExecuteAsync(StoppingCts.Token));
                executingTask.ContinueWith(primaryTask =>
                {
                    if (primaryTask.Exception != null)
                    {
                        Logger.LogError(primaryTask.Exception, $"Error: {this.GetType().Name}");
                    }

                    Logger.LogInformation(message: $"ExecuteAsync finished {this.GetType().Name} with State {primaryTask.Status}");
                });

                executingTask.Start();

                SetWorkerHasStarted();
            }
            catch (Exception exc)
            {
                Logger.LogCritical(exc, $"Error: {this.GetType().Name}");

                SetWorkerHasStopped();
            }

            return Task.CompletedTask;
        }

        #region private methods

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            SetWorkerIsStopping();

            SetWorkerHasStopped();

            return Task.CompletedTask;
        }

        #endregion

        #endregion

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}

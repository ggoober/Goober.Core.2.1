using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.BackgroundWorker
{
    public abstract class BaseBackgroundWorker : IHostedService
    {
        #region fields

        protected string _id = Guid.NewGuid().ToString();
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private Stopwatch _serviceWatch = new Stopwatch();
        private Stopwatch _taskWatch = new Stopwatch();

        #endregion


        #region protected properties

        protected IServiceProvider ServiceProvider { get; private set; }

        protected IServiceScopeFactory ServiceScopeFactory { get; private set; }

        protected ILogger Logger { get; private set; }

        #endregion


        #region public properties

        public bool IsRunning { get; protected set; } = false;

        public TimeSpan ServiceUpTime => _serviceWatch.Elapsed;

        public TimeSpan TaskUpTime => _taskWatch.Elapsed;

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
            Logger.LogInformation($"BackgroundWorker ({_id}) starting...");

            _serviceWatch.Start();

            try
            {
                _taskWatch.Start();

                _executingTask = ExecuteAsync(_stoppingCts.Token);

                IsRunning = true;
                
                Logger.LogInformation($"BackgroundWorker ({_id}) has started.");
            }
            catch (Exception exc)
            {
                Logger.LogCritical(exc, $"BackgroundWorker ({_id}) start fail");

                IsRunning = false;
                _taskWatch.Stop();
            }

            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {

            Logger.LogInformation($"BackgroundWorker ({_id}) stoping...");

            if (_executingTask == null)
            {
                return;
            }

            _stoppingCts.Cancel();

            Logger.LogInformation($"BackgroundWorker ({_id}) stoping: waiting executed task");

            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            Logger.LogInformation($"BackgroundWorker ({_id}) finalized");

            cancellationToken.ThrowIfCancellationRequested();
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}

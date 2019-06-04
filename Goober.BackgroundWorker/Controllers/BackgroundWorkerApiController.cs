using Goober.BackgroundWorker.BackgroundServices;
using Goober.BackgroundWorker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.BackgroundWorker.Controllers
{
    public class BackgroundWorkerApiController: Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public BackgroundWorkerApiController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost]
        [Route("api/backgroundworker/ping")]
        public virtual PingApiResponse Ping()
        {
            var ret = new PingApiResponse();

            var workers = _serviceProvider.GetServices<IHostedService>();

            foreach (var iWorker in workers)
            {
                var backgroundWorker = iWorker as BaseBackgroundWorker;
                if (backgroundWorker == null)
                    continue;

                var newWorker = new BackgroundWorkerPingModel
                {
                    IsRunning = backgroundWorker.IsRunning,
                    Name = backgroundWorker.GetType().FullName,
                    ServiceUpTime = Convert.ToInt64(backgroundWorker.ServiceUpTime.TotalSeconds),
                    TaskUpTime = Convert.ToInt64(backgroundWorker.TaskUpTime.TotalSeconds)
                };

                var iterateBackgroundWorker = backgroundWorker as IIterateBackgroundMetrics;

                if (iterateBackgroundWorker != null)
                {
                    newWorker.IteratedCount = iterateBackgroundWorker.IteratedCount;
                    newWorker.SuccessIteratedCount = iterateBackgroundWorker.SuccessIteratedCount;
                    newWorker.LastIterationDurationInMilliseconds = iterateBackgroundWorker.LastIterationDurationInMilliseconds;
                    newWorker.LastIterationFinishDateTime = iterateBackgroundWorker.LastIterationFinishDateTime;
                    newWorker.LastIterationStartDateTime = iterateBackgroundWorker.LastIterationStartDateTime;
                }

                var listBackgroundWorker = backgroundWorker as IListBackgroundMetrics;
                if (listBackgroundWorker != null)
                {
                    newWorker.MaxParallelTasks = listBackgroundWorker.MaxParallelTasks;
                    newWorker.LastIterationListItemsCount = listBackgroundWorker.LastIterationListItemsCount;
                    newWorker.LastIterationListItemsProcessedCount = listBackgroundWorker.LastIterationListItemsProcessedCount;
                    newWorker.LastIterationListItemsSuccessProcessedCount = listBackgroundWorker.LastIterationListItemsSuccessProcessedCount;
                    newWorker.LastIterationListItemsLastDurationInMilliseconds = listBackgroundWorker.LastIterationListItemsLastDurationInMilliseconds;
                    newWorker.LastIterationListItemsAvgDurationInMilliseconds = listBackgroundWorker.LastIterationListItemsAvgDurationInMilliseconds;
                    newWorker.LastIterationListItemExecuteDateTime = listBackgroundWorker.LastIterationListItemExecuteDateTime;
                }

                ret.Services.Add(newWorker);
            }

            return ret;
        }

        [HttpPost]
        [Route("api/backgroundworker/start")]
        public virtual async Task StartBackgroundWorker([FromBody]string backgroundWorkerFullName)
        {
            if (string.IsNullOrEmpty(backgroundWorkerFullName) == true)
                throw new ArgumentNullException("request.FullName");

            var worker = GetBackgroundWorkerByFullName(backgroundWorkerFullName);

            if (worker == null)
                throw new InvalidOperationException($"Can't find backgroundWorker by name = {backgroundWorkerFullName}");

            await worker.StartAsync(new CancellationToken());
        }

        [HttpPost]
        [Route("api/backgroundworker/stop")]
        public virtual async Task StopBackgroundWorker([FromBody]string backgroundWorkerFullName)
        {
            var worker = GetBackgroundWorkerByFullName(backgroundWorkerFullName);

            if (worker == null)
                throw new InvalidOperationException($"Can't find backgroundWorker by name = {backgroundWorkerFullName}");

            await worker.StopAsync(new CancellationToken());
        }


        private BaseBackgroundWorker GetBackgroundWorkerByFullName(string fullName)
        {
            var workers = _serviceProvider.GetServices<IHostedService>();

            foreach (var iWorker in workers)
            {
                var backgroundWorker = iWorker as BaseBackgroundWorker;
                if (backgroundWorker == null)
                    continue;

                if (backgroundWorker.GetType().FullName == fullName)
                    return backgroundWorker;
            }

            return null;
        }
    }
}

using BackgroundWorkers.Services;
using Goober.BackgroundWorker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundWorkers.BackgroundWorkers
{
    public class ListTestBackgroundWorker : ListBackgroundWorker<int, IListTestBackgroundService>
    {
        public ListTestBackgroundWorker(ILogger<ListTestBackgroundWorker> logger, IServiceProvider serviceProvider) 
            : base(logger, serviceProvider)
        {
        }
    }
}

using Goober.BackgroundWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundWorkers.Services
{
    public interface IListTestBackgroundService: IListBackgroundService<int>
    {
    }
}

using System.Collections.Generic;

namespace Goober.BackgroundWorker.Options
{
    public class BackgroundWorkersOptions
    {
        public List<BackgroundWorkerOptionModel> Workers { get; set; } = new List<BackgroundWorkerOptionModel>();
    }
}

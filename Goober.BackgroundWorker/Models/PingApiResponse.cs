using System.Collections.Generic;

namespace Goober.BackgroundWorker.Models
{
    public class PingApiResponse
    {
        public List<BackgroundWorkerPingModel> Services { get; set; } = new List<BackgroundWorkerPingModel>();
    }
}

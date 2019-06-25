using System.Threading;
using System.Threading.Tasks;

namespace Goober.BackgroundWorker
{
    public interface IIterateBackgroundService
    {
        Task ExecuteIterationAsync(CancellationToken stoppingToken);
    }
}

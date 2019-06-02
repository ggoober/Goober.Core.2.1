using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goober.BackgroundWorker
{
    public interface IListBackgroundService<TItem>
    {
        Task<List<TItem>> GetItemsAsync();

        Task ProcessItemAsync(TItem item);
    }
}

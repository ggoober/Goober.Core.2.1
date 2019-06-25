using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundWorkers.Services.Implementation
{
    public class ListTestBackgroundService : IListTestBackgroundService
    {
        public ListTestBackgroundService()
        {
        }

        public async Task<List<int>> GetItemsAsync()
        {
            Thread.Sleep(5000);

            return new List<int> { 3, 4, 5, 6, 7 };
        }

        public async Task ProcessItemAsync(int item, CancellationToken stoppinngToken)
        {
            Thread.Sleep(item * 1000);
        }
    }
}

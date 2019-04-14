using System;
using System.Threading.Tasks;

namespace Goober.Core.Services
{
    public interface ICacheProvider
    {
        void Remove(string cacheKey);

        T Get<T>(string cacheKey, int cacheTimeInMinutes, Func<T> func);

        Task<T> GetAsync<T>(string cacheKey, int cacheTimeInMinutes, Func<Task<T>> func);
    }
}

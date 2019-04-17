using Goober.Core.Services;
using Goober.Core.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Goober.Core.Extensions
{
    public static class CachingExtensions
    {
        public static void AddCaching(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, CacheProvider>();
        }
    }
}

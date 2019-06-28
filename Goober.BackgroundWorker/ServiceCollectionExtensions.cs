using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Goober.BackgroundWorker
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBackgroundWorkers<TAssemblyClassName>(this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "BackgroundWorkers")
        {
            services.Configure<BackgroundWorkersOptions>(configuration.GetSection(sectionName));

            services.RegisterBackgroundWorkers(typeof(TAssemblyClassName).Assembly);
        }

        private static void RegisterBackgroundWorkers(this IServiceCollection services,
            Assembly assembly)
        {
            var backgroundWorkers = new List<Type>();

            foreach (var assemblyDefinedType in assembly.GetTypes())
            {
                if (assemblyDefinedType.Name.EndsWith("BackgroundWorker"))
                {
                    if (assemblyDefinedType.IsClass == false)
                        continue;

                    if (assemblyDefinedType.GetInterfaces().Contains(typeof(IHostedService)) == false)
                        continue;

                    backgroundWorkers.Add(assemblyDefinedType);
                }
            }

            foreach (var implementType in backgroundWorkers)
            {
                services.Add(new ServiceDescriptor(typeof(IHostedService), implementType, ServiceLifetime.Singleton));
            }
        }
    }
}

using Goober.BackgroundWorker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Goober.BackgroundWorker
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBackgroundWorkers(this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "BackgroundWorkers")
        {
            services.Configure<BackgroundWorkersOptions>(configuration.GetSection(sectionName));
        }



        public static void RegisterClasses(this IServiceCollection services,
            Assembly assembly,
            List<string> classesPostfix,
            ServiceLifetime serviceLifetime,
            bool optional)
        {
            if (classesPostfix == null || classesPostfix.Any() == false)
                throw new InvalidOperationException();
            var implementTypes = new List<Type>();

            foreach (var assemblyDefinedType in assembly.GetTypes())
            {
                if (classesPostfix.Any(x => assemblyDefinedType.Name.EndsWith(x)))
                {
                    var attribute = assemblyDefinedType.GetCustomAttributes(typeof(ServiceCollectionIgnoreRegistrationAttribute), false).FirstOrDefault();

                    if (attribute != null)
                        continue;

                    if (assemblyDefinedType.IsClass == false)
                        continue;

                    implementTypes.Add(assemblyDefinedType);
                }
            }

            foreach (var implementType in implementTypes)
            {
                var interfaceName = "I" + implementType.Name;
                var interfaceType = implementType.GetInterface(interfaceName);

                if (interfaceType == null && optional == false)
                    throw new InvalidOperationException($"Can't find interface = {interfaceName} for class.Name = {implementType.Name}, class.FullName = {implementType.FullName}. Add 'HermesIgnoreRegistrationAttribute' if need ");

                services.Add(new ServiceDescriptor(interfaceType, implementType, serviceLifetime));
            }
        }

        public static void RegisterAssemblyClasses<TAssemblyClassName>(this IServiceCollection services, List<string> classesPostfix = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, bool optional = false)
            where TAssemblyClassName : class
        {
            RegisterClasses(services: services,
                assembly: typeof(TAssemblyClassName).Assembly,
                classesPostfix: classesPostfix ?? ServiceAndRepositoryPostfix,
                serviceLifetime: serviceLifetime,
                optional: optional);
        }
    }
}

using BackgroundWorkers.BackgroundWorkers;
using BackgroundWorkers.Services;
using Goober.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackgroundWorkers
{
    public class Startup: GooberStartup
    {
        public Startup(IHostingEnvironment env, IConfiguration config) : base(env, config)
        {
        }

        protected override void ConfigureServiceCollections(IServiceCollection services)
        {
            services.AddSingleton<IHostedService, IterateTestBackgroundWorker>();

            Goober.Core.Extensions.ServiceCollectionExtensions.RegisterAssemblyClasses<IIterateTestBackgroundService>(services);
        }

        protected override void ConfigurePipelineAfterExceptionsHandling(IApplicationBuilder app)
        {
        }

        protected override void ConfigurePipelineAfterMvc(IApplicationBuilder app)
        {
        }
    }
}

using BackgroundWorkers.Services;
using Goober.BackgroundWorker;
using Goober.Core.Extensions;
using Goober.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddBackgroundWorkers<IIterateTestBackgroundService>(Configuration);
            services.RegisterAssemblyClasses<IIterateTestBackgroundService>();
        }

        protected override void ConfigurePipelineAfterExceptionsHandling(IApplicationBuilder app)
        {
        }

        protected override void ConfigurePipelineAfterMvc(IApplicationBuilder app)
        {
        }
    }
}

using Goober.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMq
{
    public class Startup : Goober.WebApi.GooberStartup
    {
        public Startup(IHostingEnvironment env, IConfiguration config) 
            : base(env, config)
        {
        }

        protected override void ConfigurePipelineAfterExceptionsHandling(IApplicationBuilder app)
        {
        }

        protected override void ConfigurePipelineAfterMvc(IApplicationBuilder app)
        {
        }

        protected override void ConfigureServiceCollections(IServiceCollection services)
        {
            //services.AddScoped<IEventTypeAHandler, EventTypeAHandler>();
            //services.AddScoped<IEventTypeBHandler, EventTypeBHandler>();
            services.AddScoped<EventTypeAHandler>();
            services.AddRabbitMq(Configuration);
        }
    }
}

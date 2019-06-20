using Goober.RabbitMq.BackgroundWorkers;
using Goober.RabbitMq.Options;
using Goober.RabbitMq.Services;
using Goober.RabbitMq.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Goober.RabbitMq
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRabbitMq(this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "RabbitMq")
        {
            services.AddOptions();
            services.AddLogging();
            services.AddSingleton<IHostedService, RabbitMqConsumerBackgroundWorker>();
            services.AddSingleton<IRabbitMqMessageProducer, RabbitMqMessageProducer>();
            services.Configure<RabbitMqClientOptions>(configuration.GetSection(sectionName));
        }
    }
}

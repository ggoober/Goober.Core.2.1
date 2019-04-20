using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Reflection;

namespace Goober.Logging
{
    public static class LoggingConfigurationExtensions
    {
        public static void UseExtenedLoggingVariables(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware(typeof(LoggingMiddleware));
        }

        public static void UseNlogExceptionsHandling(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware(typeof(ErrorHandlingMiddleware));
        }

        public static IWebHostBuilder UseNLogExtended(this IWebHostBuilder webHostBuilder, string environmentVariable = "ASPNETCORE_ENVIRONMENT")
        {
            var nLogConfiguration = NLog.LogManager.Configuration;

            if (nLogConfiguration != null)
            {
                nLogConfiguration.Variables["ENVIRONMENT"] = Environment.GetEnvironmentVariable(environmentVariable);
                nLogConfiguration.Variables["APPLICATION"] = Assembly.GetEntryAssembly().GetName().Name;
            }

            return webHostBuilder
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                }
                )
                .UseNLog();
        }
    }
}

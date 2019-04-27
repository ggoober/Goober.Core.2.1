using Goober.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Goober.WebApp
{
    public static class ProgramUtils
    {
        public static void RunWebhost<TStartup>(string[] args, string nlogConfigFileName = "nlog.config")
            where TStartup : class
        {
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                var webHost = CreateWebHostBuilder<TStartup>(args)
                    .Build();

                webHost.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder<TStartup>(string[] args)
            where TStartup: class
        {
            return WebHost.CreateDefaultBuilder(args)
                             .UseNLogExtended()
                             .UseStartup<TStartup>();
        }
    }
}

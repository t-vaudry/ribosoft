using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Ribosoft.Data;

namespace Ribosoft
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildMainWebHost(args);
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    ApplicationDbInitializer.Initialize(services).Wait();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                    throw;
                }
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static IWebHost BuildMainWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddCommandLine(args)
                .Build();
            
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .ConfigureServices(services => services.AddTransient<IStartupFilter, HangfireStartupFilter>())
                .UseStartup<Startup>()
                .UseNLog()
                .Build();
        }
    }
}

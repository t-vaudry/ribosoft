using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Ribosoft.Data;

//!< \namespace Ribosoft
namespace Ribosoft
{
    /*! \class Program
     * \brief Object class for the Program
     */
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /*! \fn Main 
         * \brief Main program function
         * Sets up the main web host and the logger
         * Also initializes the application database
         * \param args Array of string arguments
         */
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

        /*! \fn BuildWebHost 
         * \brief Function to build the web host
         * \param args Array of string arguments
         * \return obj IWebHost object
         */
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        /*! \fn BuildMainWebHost 
         * \brief Function to build the main web host
         * \param args Array of string arguments
         * \return obj IWebHost object
         */
        public static IWebHost BuildMainWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .AddCommandLine(args)
                .Build();
            
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .ConfigureServices(services => services.AddTransient<IStartupFilter, HangfireStartupFilter>() .AddMvc(option => option.EnableEndpointRouting = false))
                .UseStartup<Startup>()
                .UseNLog()
                .Build();
        }
    }
}

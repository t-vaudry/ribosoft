using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ribosoft.Jobs;

//!< \namespace Ribosoft
namespace Ribosoft
{
    /*! \class HangfireStartupFilter
     * \brief This class is used to configure Hangfire for the application.
     */
    [ExcludeFromCodeCoverage]
    public class HangfireStartupFilter : IStartupFilter
    {
        /*! \property _configuration
         * \brief IConfiguration object for Hangfire setup
         */
        private readonly IConfiguration _configuration;
        
        /*!
         * \brief Default constructor
         * \param configuration IConfiguration object for Hangfire setup
         */
        public HangfireStartupFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /*! \fn Configure
         * \brief Configuration function
         * \param next Action for ApplicationBuilder
         * \return builder Action<IApplicationBuiler>
         */
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // call next() first to ensure the HttpContext is initialized for the Hangfire Auth filter
                next(builder);

                GlobalConfiguration.Configuration.UseActivator(new ServiceProviderActivator(builder.ApplicationServices));

                int workerCount = string.IsNullOrEmpty(_configuration["Hangfire:WorkerCount"])
                    ? 1
                    : int.Parse(_configuration["Hangfire:WorkerCount"]);

                string[] queues = string.IsNullOrEmpty(_configuration["Hangfire:Queues"])
                    ? new[] {"blast", "default"}
                    : _configuration["Hangfire:Queues"].Split(',');
                
                var options = new BackgroundJobServerOptions { WorkerCount = workerCount, Queues = queues };

                builder.UseHangfireServer(options);
                builder.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new DashboardAuthorizationFilter() }
                });
            };
        }
    }
}

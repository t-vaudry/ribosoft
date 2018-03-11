using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ribosoft.Jobs;

namespace Ribosoft
{
    public class HangfireStartupFilter : IStartupFilter
    {
        private readonly IConfiguration _configuration;
        
        public HangfireStartupFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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

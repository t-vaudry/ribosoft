using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Ribosoft.Jobs;

namespace Ribosoft
{
    public class HangfireStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // call next() first to ensure the HttpContext is initialized for the Hangfire Auth filter
                next(builder);

                GlobalConfiguration.Configuration.UseActivator(new ServiceProviderActivator(builder.ApplicationServices));

                var options = new BackgroundJobServerOptions { WorkerCount = Math.Max(Environment.ProcessorCount / 2, 1) };

                builder.UseHangfireServer(options);
                builder.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new DashboardAuthorizationFilter() }
                });
            };
        }
    }
}

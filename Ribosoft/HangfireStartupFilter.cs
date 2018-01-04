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
                GlobalConfiguration.Configuration.UseActivator(new ServiceProviderActivator(builder.ApplicationServices));

                builder.UseHangfireServer();
                builder.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new DashboardAuthorizationFilter() }
                });

                next(builder);
            };
        }
    }
}

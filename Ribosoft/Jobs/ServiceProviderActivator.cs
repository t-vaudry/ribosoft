using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

namespace Ribosoft.Jobs
{
    public class ServiceProviderActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}

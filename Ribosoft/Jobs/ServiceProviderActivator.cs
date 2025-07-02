using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

//! \namespace Ribosoft.Jobs
namespace Ribosoft.Jobs
{
    /*! \class ServiceProviderActivator
     * \brief This job is used to activate service providers of the system.
     */
    public class ServiceProviderActivator : JobActivator
    {
        /*! \property _serviceProvider
         * \brief Local Service Provider
         */
        private readonly IServiceProvider _serviceProvider;

        /*! \fn ServiceProviderActivator
         * \brief Default constructor
         * \param serviceProvider Service Provider
         */
        public ServiceProviderActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /*! \fn ActivateJob
         * \brief Function to activate specified job
         * \param type Job type
         * \return Job object
         */
        public override object ActivateJob(Type type)
        {
            return _serviceProvider.GetService(type) ?? throw new InvalidOperationException($"Unable to create job of type {type.Name}");
        }
    }
}

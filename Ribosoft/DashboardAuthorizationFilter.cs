using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Dashboard;

namespace Ribosoft
{
    /*! \class DashboardAuthorizationFilter
     * \brief Filter class for the authorization to view the dashboards
     */
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /*! \fn Authorize
         * \brief Authorization function to access dashboards
         * \param context Dashboard context for retrieval
         * \return bool If user is an Administrator
         */
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.IsInRole("Administrator");
        }
    }
}

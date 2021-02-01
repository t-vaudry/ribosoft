using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

//! \namespace Ribosoft.Models.ManageViewModels
namespace Ribosoft.Models.ManageViewModels
{
    /*! \class ExternalLoginsViewModel
     * \brief This model is currently not used by the product.
     */
    public class ExternalLoginsViewModel
    {
        /*! \property CurrentLogins
         * \brief List of current external logins
         */
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        /*! \property OtherLogins
         * \brief List of other logins
         */
        public IList<AuthenticationScheme> OtherLogins { get; set; }

        /*! \property ShowRemoveButton
         * \brief Boolean for display of remove button
         */
        public bool ShowRemoveButton { get; set; }

        /*! \property StatusMessage
         * \brief Status message
         */
        public string StatusMessage { get; set; }
    }
}

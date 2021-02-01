using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
	/*! \class TwoFactorAuthenticationViewModel
     * \brief Model class for the two factor authentication view
     */
    public class TwoFactorAuthenticationViewModel
    {
    	/*! \property HasAuthenticator
         * \brief Check for aunthenticator
         */
        public bool HasAuthenticator { get; set; }

        /*! \property RecoveryCodesLeft
         * \brief Remaining recovery codes
         */
        public int RecoveryCodesLeft { get; set; }

        /*! \property Is2faEnabled
         * \brief Check for two-factor authentication state
         */
        public bool Is2faEnabled { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
    /*! \class EnableAuthenticatorViewModel
     * \brief Model class for the enable authenticator view
     */
    public class EnableAuthenticatorViewModel
    {
        /*! \property Code
         * \brief Verification code
         */
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }

        /*! \property SharedKey
         * \brief Read-only shared key
         */
        [ReadOnly(true)]
        public string SharedKey { get; set; }

        /*! \property AuthenticatorUri
         * \brief Authenticator URI
         */
        public string AuthenticatorUri { get; set; }
    }
}

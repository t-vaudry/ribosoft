using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
    /*! \class LoginWith2faViewModel
     * \brief Model class for the login with two-factor view
     */
    public class LoginWith2faViewModel
    {
        /*! \property TwoFactorCode
         * \brief Authenticator code
         */
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; } = string.Empty;

        /*! \property RememberMachine
         * \brief Remember this machine flag
         */
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        /*! \property RememberMe
         * \brief Remember me flag
         */
        public bool RememberMe { get; set; }
    }
}

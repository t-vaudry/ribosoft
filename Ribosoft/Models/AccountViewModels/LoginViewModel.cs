using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
    /*! \class LoginViewModel
     * \brief Model class for the login view for account and guest members
     */
    public class LoginViewModel
    {
        /*! \property Email
         * \brief Email address
         */
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /*! \property Password
         * \brief User password
         */
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /*! \property RememberMe
         * \brief Remember me flag
         */
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        /*! \property Guest
         * \brief Guest flag
         */
        [Display(Name = "Please note: Guests are not able to return to view jobs after leaving the web service. To view job results, keep the Job Id on hand, and it can be reviewed when returning. Use for short jobs; longer jobs should be done with a full login.")]
        public bool Guest { get; set; }
    }
}

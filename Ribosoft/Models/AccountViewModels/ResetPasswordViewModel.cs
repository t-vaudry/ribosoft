using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
    /*! \class ResetPasswordViewModel
     * \brief Model class for the reset password view for an account
     */
    public class ResetPasswordViewModel
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
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /*! \property ConfirmPassword
         * \brief User confirm password
         */
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /*! \property Code
         * \brief Code
         */
        public string Code { get; set; } = string.Empty;
    }
}

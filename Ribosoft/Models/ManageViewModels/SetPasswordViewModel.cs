using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
    /*! \class SetPasswordViewModel
     * \brief Model class for the set password view
     */
    public class SetPasswordViewModel
    {
        /*! \property NewPassword
         * \brief User new password
         */
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        /*! \property ConfirmPassword
         * \brief User confirm password
         */
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        /*! \property StatusMessage
         * \brief Status message
         */
        public string StatusMessage { get; set; } = string.Empty;
    }
}

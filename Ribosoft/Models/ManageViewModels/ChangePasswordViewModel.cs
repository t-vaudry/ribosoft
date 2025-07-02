using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
    /*! \class ChangePasswordViewModel
     * \brief Model class for the change password view
     */
    public class ChangePasswordViewModel
    {
        /*! \property OldPassword
         * \brief Current password
         */
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = string.Empty;

        /*! \property vm
         * \brief Set password view model object
         */
        public SetPasswordViewModel vm { get; set; } = new SetPasswordViewModel();
    }
}

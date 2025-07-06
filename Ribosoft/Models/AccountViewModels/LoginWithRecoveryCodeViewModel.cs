using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
	/*! \class LoginWithRecoveryCodeViewModel
     * \brief Model class for the login with recovery code view
     */
    public class LoginWithRecoveryCodeViewModel
    {
        /*! \property RecoveryCode
         * \brief Recovery code
         */
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = string.Empty;
    }
}

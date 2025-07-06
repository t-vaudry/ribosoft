using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
	/*! \class ForgotPasswordViewModel
     * \brief Model class for the forgot password view
     */
    public class ForgotPasswordViewModel
    {
    	/*! \property Email
         * \brief Email address
         */
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

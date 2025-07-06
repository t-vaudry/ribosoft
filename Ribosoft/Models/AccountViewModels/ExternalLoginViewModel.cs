using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
    /*! \class ExternalLoginViewModel
     * \brief Model class for the external login view
     */
    public class ExternalLoginViewModel
    {
    	/*! \property Email
         * \brief Email address
         */
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}

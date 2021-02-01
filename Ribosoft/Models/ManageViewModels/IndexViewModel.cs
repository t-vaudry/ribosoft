using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
    /*! \class IndexViewModel
     * \brief Model class for the index view
     */
    public class IndexViewModel
    {
        /*! \property Username
         * \brief Username
         */
        public string Username { get; set; }

        /*! \property IsEmailConfirmed
         * \brief Is the user's email confirmed?
         */
        public bool IsEmailConfirmed { get; set; }

        /*! \property Email
         * \brief Email address
         */
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /*! \property PhoneNumber
         * \brief User phone number
         */
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        /*! \property StatusMessage
         * \brief Status message
         */
        public string StatusMessage { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Display(Name = "Please note: Guests are not able to return to view jobs after leaving the web service. To view job results, keep the Job Id on hand, and it can be reviewed when returning. Use for short jobs; longer jobs should be done with a full login.")]
        public bool Guest { get; set; }
    }
}

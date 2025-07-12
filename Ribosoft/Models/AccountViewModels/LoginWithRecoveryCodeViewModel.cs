using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.AccountViewModels
{
	/*! \class LoginWithRecoveryCodeViewModel
     * \brief Model class for the login with one-time email code view
     */
    public class LoginWithRecoveryCodeViewModel
    {
        /*! \property OneTimeCode
         * \brief One-time code sent via email
         */
        [Required(ErrorMessage = "Please enter the verification code.")]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits.")]
        public string OneTimeCode { get; set; } = string.Empty;

        /*! \property Email
         * \brief Email address for display purposes (read-only)
         */
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /*! \property CodeSent
         * \brief Indicates if a code has been sent
         */
        public bool CodeSent { get; set; } = false;

        /*! \property CanResend
         * \brief Indicates if user can request a new code
         */
        public bool CanResend { get; set; } = true;

        /*! \property ResendCooldownSeconds
         * \brief Seconds remaining before user can request another code
         */
        public int ResendCooldownSeconds { get; set; } = 0;
    }
}

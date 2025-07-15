using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ribosoft.Extensions;
using Ribosoft.Models;
using Ribosoft.Models.ManageViewModels;
using Ribosoft.Services;

namespace Ribosoft.Controllers
{
    /*! \class ManageController
     * \brief Controller class for the management of accounts
     */
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        /*! \property _userManager
         * \brief Local application user manager
         */
        private readonly UserManager<ApplicationUser> _userManager;

        /*! \property _signInManager
         * \brief Local application sign in manager
         */
        private readonly SignInManager<ApplicationUser> _signInManager;

        /*! \property _emailSender
         * \brief Local application email sender
         */
        private readonly IEmailSender _emailSender;

        /*! \property _logger
         * \brief Local logging service
         */
        private readonly ILogger _logger;

        /*! \property _urlEncoder
         * \brief Local URL encoder
         */
        private readonly UrlEncoder _urlEncoder;

        /*! \property AuthenticatorUriFormat
         * \brief Authenticator URI formate
         */
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        /*! \property RecoveryCodesKey
         * \brief String name of RecoveryCodesKey
         */
        private const string RecoveryCodesKey = nameof(RecoveryCodesKey);

        /*! \fn ManageController
         * \brief Default constructor
         * \param userManager Application user manager
         * \param signInManager Application sign in manager
         * \param emailSender Application email sender
         * \param logger Logging service
         * \param urlEncoder URL encoder
         */
        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        /*! \property StatusMessage
         * \brief Status message stored in temp data
         */
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        /*!
         * \brief HTTP GET request for the manage account page
         * \return View of the manage account index
         */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new IndexViewModel
            {
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        /*!
         * \brief HTTP POST request to update account information
         * \param model Model object of index view
         * \return View of the manage account index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToAction(nameof(Index));
        }

        /*! \fn SendVerificationEmail
         * \brief HTTP GET request to send verification email for account
         * \return Redirect to manage account index with status message
         */
        [HttpGet]
        public async Task<IActionResult> SendVerificationEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if email is already confirmed
            if (user.EmailConfirmed)
            {
                StatusMessage = "Your email is already verified.";
                return RedirectToAction(nameof(Index));
            }

            // Check if user has an email address
            if (string.IsNullOrEmpty(user.Email))
            {
                StatusMessage = "Error: No email address found. Please update your profile with a valid email address.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Generate email confirmation token and send verification email
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);

                _logger.LogInformation("Verification email sent to user {UserId} at {Email}", user.Id, user.Email);
                StatusMessage = "Verification email sent successfully! Please check your email and click the confirmation link.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to user {UserId}", user.Id);
                StatusMessage = "Error: Failed to send verification email. Please try again later.";
            }

            return RedirectToAction(nameof(Index));
        }

        /*!
         * \brief HTTP GET request for change password page
         * \return View of the change password index
         */
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel { vm = new SetPasswordViewModel { StatusMessage = StatusMessage } };
            return View(model);
        }

        /*!
         * \brief HTTP POST request to submit change of password
         * \param model Model object of change password view
         * \return View of the change password index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.vm.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }

        /*!
         * \brief HTTP GET request for set password page
         * \return View of the set password index
         */
        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }

        /*!
         * \brief HTTP POST request to set password
         * \param model Model object of the set password view
         * \return View of the set password index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been set.";

            return RedirectToAction(nameof(SetPassword));
        }

        /*! \fn ExternalLogins
         * \brief HTTP GET request to the external logins page
         * \return View of the external logins index
         */
        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            model.StatusMessage = StatusMessage;

            return View(model);
        }

        /*! \fn LinkLogin
         * \brief HTTP POST request to link an external login to the current user
         * \param provider External login provider
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback));
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        /*! \fn LinkLoginCallback
         * \brief HTTP GET request to add the external login
         * \return View of the external logins index
         */
        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
            {
                throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        /*! \fn RemoveLogin
         * \brief HTTP POST request to remove the external login
         * \param model Model object of the remove login view
         * \return View of the external logins index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        /*! \fn TwoFactorAuthentication
         * \brief HTTP GET request for two-factor authentication page
         * \return View of the two-factor authentication index
         */
        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            };

            return View(model);
        }

        /*! \fn Disable2faWarning
         * \brief HTTP GET request for disabled two-factor authentication warning page
         * \return View of the disabled two-factor authentication index
         */
        [HttpGet]
        public async Task<IActionResult> Disable2faWarning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            return View(nameof(Disable2fa));
        }

        /*! \fn Disable2fa
         * \brief HTTP POST request to disable two-factor authentication
         * \return View of the two-factor authentication index or JSON response for AJAX
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "User not found" });
                }
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Unexpected error occurred disabling 2FA" });
                }
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            _logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Two-factor authentication has been disabled" });
            }
            
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        /*!
         * \brief HTTP GET request for enable authenticator page
         * \return View of the enable authenticator index
         */
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new EnableAuthenticatorViewModel();
            await LoadSharedKeyAndQrCodeUriAsync(user, model);

            return View(model);
        }

        /*!
         * \brief HTTP POST request to enable two-factor authentication
         * \param model Model object of the enable authenticator view
         * \return View of the show recovery codes index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            _logger.LogInformation("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            TempData[RecoveryCodesKey] = (recoveryCodes ?? Enumerable.Empty<string>()).ToArray();

            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        /*! \fn ShowRecoveryCodes
         * \brief HTTP GET request for the show recovery codes page
         * \return View of the show recovery codes index
         */
        [HttpGet]
        public IActionResult ShowRecoveryCodes()
        {
            var recoveryCodes = (string[]?)TempData[RecoveryCodesKey];
            if (recoveryCodes == null)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            var model = new ShowRecoveryCodesViewModel { RecoveryCodes = recoveryCodes };
            return View(model);
        }

        /*! \fn ResetAuthenticatorWarning
         * \brief HTTP GET request for the reset authenticator warning page
         * \return View of the reset authenticator index
         */
        [HttpGet]
        public IActionResult ResetAuthenticatorWarning()
        {
            return View(nameof(ResetAuthenticator));
        }

        /*! \fn ResetAuthenticator
         * \brief HTTP POST request to reset the authentication app key
         * \return View of the enable authenticator index or JSON response for AJAX
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "User not found" });
                }
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Authenticator key has been reset" });
            }

            return RedirectToAction(nameof(EnableAuthenticator));
        }

        /*! \fn GenerateRecoveryCodesWarning
         * \brief HTTP GET request for the generate recovery codes warning page
         * \return View of the generate recovery codes index
         */
        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodesWarning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' because they do not have 2FA enabled.");
            }

            return View(nameof(GenerateRecoveryCodes));
        }

        /*! \fn GenerateRecoveryCodes
         * \brief HTTP POST request to generate new two-factor authentication recovery codes
         * \return View of the show recovery codes index or JSON response for AJAX
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "User not found" });
                }
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Cannot generate recovery codes as 2FA is not enabled" });
                }
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            _logger.LogInformation("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true, 
                    codes = recoveryCodes?.ToArray() ?? new string[0],
                    message = "New recovery codes generated successfully"
                });
            }

            var model = new ShowRecoveryCodesViewModel { RecoveryCodes = (recoveryCodes ?? Enumerable.Empty<string>()).ToArray() };
            return View(nameof(ShowRecoveryCodes), model);
        }

        /*! \fn GetAuthenticatorData
         * \brief HTTP POST request to get authenticator setup data as JSON
         * \return JSON with shared key and authenticator URI
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAuthenticatorData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found" });
            }

            try
            {
                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(unformattedKey))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                }

                var sharedKey = FormatKey(unformattedKey ?? "");
                var authenticatorUri = GenerateQrCodeUri(user.Email ?? "", unformattedKey ?? "");

                return Json(new { 
                    success = true, 
                    sharedKey = sharedKey, 
                    authenticatorUri = authenticatorUri 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating authenticator data for user {UserId}", user.Id);
                return Json(new { success = false, error = "Error generating authenticator data" });
            }
        }

        /*! \fn GetRecoveryCodes
         * \brief HTTP GET request to get current recovery codes as JSON
         * \return JSON with recovery codes
         */
        [HttpGet]
        public async Task<IActionResult> GetRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found" });
            }

            if (!user.TwoFactorEnabled)
            {
                return Json(new { success = false, error = "Two-factor authentication is not enabled" });
            }

            try
            {
                // Get the count of remaining recovery codes
                var recoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
                
                if (recoveryCodesLeft == 0)
                {
                    return Json(new { 
                        success = false, 
                        error = "No recovery codes available. Please generate new codes.",
                        needsGeneration = true 
                    });
                }

                // Note: ASP.NET Core Identity doesn't provide a way to retrieve the actual 
                // recovery codes for security reasons. They are hashed and stored securely.
                // We can only show the count and suggest generating new ones.
                return Json(new { 
                    success = false, 
                    error = $"You have {recoveryCodesLeft} recovery codes remaining, but the actual codes cannot be displayed for security reasons. You can generate new codes if needed.",
                    recoveryCodesLeft = recoveryCodesLeft,
                    cannotDisplay = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recovery codes for user {UserId}", user.Id);
                return Json(new { success = false, error = "Error retrieving recovery codes" });
            }
        }

        /*! \fn AddErrors
         * \brief Helper function to add errors to the model
         * \param result Result with errors to add
         */
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        /*! \fn FormatKey
         * \brief Helper function to format key
         * \param unformattedKey Unformatted key
         * \return Formatted key
         */
        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        /*! \fn GenerateQrCodeUri
         * \brief Helper function to generate the QR code URI
         * \param email Email address
         * \param unformattedKey Unformatted key
         * \return String of QR code URI
         */
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("Ribosoft"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        /*! \fn LoadSharedKeyAndQrCodeUriAsync
         * \brief Helper function to load shared key and the QR code URI
         * \param user Current application user
         * \param model Model object of the enable authenticator view
         */
        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user, EnableAuthenticatorViewModel model)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey ?? "");
            model.AuthenticatorUri = GenerateQrCodeUri(user.Email ?? "", unformattedKey ?? "");
        }
    }
}

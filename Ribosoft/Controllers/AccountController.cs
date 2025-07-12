using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ribosoft.Extensions;
using Ribosoft.Models;
using Ribosoft.Models.AccountViewModels;
using Ribosoft.Services;

namespace Ribosoft.Controllers
{
    /*! \class AccountController
     * \brief Controller class for the accounts
     */
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        /*! \property _userManager
         * \brief Manager of application users
         */
        private readonly UserManager<ApplicationUser> _userManager;

        /*! \property _signInManager
         * \brief Manager of sign ins
         */
        private readonly SignInManager<ApplicationUser> _signInManager;

        /*! \property _emailSender
         * \brief Sender for emails
         */
        private readonly IEmailSender _emailSender;

        /*! \property _oneTimeCodeService
         * \brief One-time code service object
         */
        private readonly IOneTimeCodeService _oneTimeCodeService;

        /*! \property _logger
         * \brief Log service object
         */
        private readonly ILogger _logger;

        /*!
         * \brief Default constructor
         */
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IOneTimeCodeService oneTimeCodeService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _oneTimeCodeService = oneTimeCodeService;
            _logger = logger;
        }

        /*! \property ErrorMessage
         * \brief Error message string used by the controller
         */
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        /*!
         * \brief HTTP GET for the login index view
         * \param returnUrl Return URL
         * \return Login view
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /*!
         * \brief HTTP POST for logging in as an account user
         * \param model Model object of the login view
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /*! \fn GuestLogin
         * \brief HTTP POST for logging in as a guest user
         * \param model Model object of the login view
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestLogin(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            Guid guid = Guid.NewGuid();
            var user = new ApplicationUser { UserName = string.Format("Guest-{0}", guid), Email = string.Format("{0}@ribosoft.com", guid) };
            var result = await _userManager.CreateAsync(user, "Ribosoft2Guest!");
            await _userManager.AddToRoleAsync(user, "Guest");
            if (result.Succeeded)
            {
                _logger.LogInformation("Guest created a new account with password.");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            AddErrors(result);
            return RedirectToLocal(returnUrl);
        }

        /*!
         * \brief HTTP GET for login view for two-factor authentication
         * \param rememberMe Boolean to remember current user
         * \param returnUrl Return URL
         * \return Login with two-factor auth view
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        /*!
         * \brief HTTP POST for login with two-factor authentication
         * \param model Model object of the login view
         * \param rememberMe Boolean to remember current user
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        /*!
         * \brief HTTP GET for logging in with one-time email code
         * \param returnUrl Return URL
         * \return View for login with one-time code
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string? returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            // Generate and send the one-time code
            await _oneTimeCodeService.GenerateAndSendCodeAsync(user.Id, user.Email!, "2FA_BYPASS");

            var model = new LoginWithRecoveryCodeViewModel
            {
                Email = user.Email!,
                CodeSent = true,
                CanResend = true,
                ResendCooldownSeconds = 0
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        /*!
         * \brief HTTP POST for logging in with one-time email code
         * \param model Model object of the login view
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            // Validate the one-time code
            var isValidCode = await _oneTimeCodeService.ValidateCodeAsync(user.Id, model.OneTimeCode, "2FA_BYPASS");

            if (isValidCode)
            {
                // Sign in the user (similar to 2FA recovery code sign-in)
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                _logger.LogInformation("User with ID {UserId} logged in with a one-time email code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                _logger.LogWarning("Invalid one-time code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid or expired verification code. Please try again or request a new code.");
                
                // Prepare model for re-display
                model.Email = user.Email!;
                model.CodeSent = true;
                model.CanResend = true;
                model.ResendCooldownSeconds = 0;
                
                return View(model);
            }
        }

        /*!
         * \brief HTTP POST for resending one-time email code
         * \param returnUrl Return URL
         * \return JSON result indicating success/failure
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOneTimeCode(string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return Json(new { success = false, message = "Session expired. Please start over." });
            }

            try
            {
                await _oneTimeCodeService.GenerateAndSendCodeAsync(user.Id, user.Email!, "2FA_BYPASS");
                _logger.LogInformation("One-time code resent for user with ID {UserId}", user.Id);
                
                return Json(new { success = true, message = "A new verification code has been sent to your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend one-time code for user with ID {UserId}", user.Id);
                return Json(new { success = false, message = "Failed to send verification code. Please try again." });
            }
        }

        /*! \fn Lockout
         * \brief HTTP GET for lockout page
         * \return View for lockout of user
         */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        /*!
         * \brief HTTP GET for registering a new account
         * \param returnUrl Return URL
         * \return View for registering new account
         */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /*!
         * \brief HTTP POST for registering a new account
         * If the user is the first in the database, they are automatically set to Administrator
         * \param model Model object of the register view
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    if (_userManager.Users.Count() <= 1)
                    {
                        await _userManager.AddToRoleAsync(user, "Administrator");
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    // Show success message with email confirmation instruction
                    ViewData["SuccessMessage"] = "Your account has been created successfully! Please check your email and click the confirmation link to activate your account before signing in.";
                    return View(new RegisterViewModel()); // Return empty model for clean form
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /*! \fn Logout
         * \brief HTTP POST for logging out
         * \return View of home page
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /*! \fn ExternalLogin
         * \brief HTTP POST for logging in as an account externally
         * \param provider External provider string
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        /*! \fn ExternalLoginCallback
         * \brief HTTP GET for callback of external login
         * \param returnUrl Return URL
         * \param remoteError String for errors from external login
         * \return View based on result
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        /*! \fn ExternalLoginConfirmation
         * \brief HTTP POST for confirming logging in externally
         * \param model Model object of the login view
         * \param returnUrl Return URL
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        /*! \fn ConfirmEmail
         * \brief HTTP GET for confirmation email
         * \param userId User's id
         * \param code Code
         * \return View based on result
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /*!
         * \brief HTTP GET for view of forget password
         * \return View for forgot password
         */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /*!
         * \brief HTTP POST for forgetting password
         * \param model Model object of the forgot password view
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await _emailSender.SendPasswordResetAsync(model.Email, callbackUrl);
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /*! \fn ForgotPasswordConfirmation
         * \brief HTTP GET for forgot password confirmation
         * \return View of login screen
         */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /*!
         * \brief HTTP GET for reset password view
         * \param code Code for password reset
         * \return View for reset password
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string? userId = null, string? code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            
            var model = new ResetPasswordViewModel { Code = code };
            
            // If userId is provided, look up the user and populate the email
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    model.Email = user.Email ?? string.Empty;
                }
            }
            
            return View(model);
        }

        /*!
         * \brief HTTP POST for resetting password
         * \param model Model object of the reset password view
         * \return View based on result
         */
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View(model);
        }

        /*! \fn ResetPasswordConfirmation
         * \brief HTTP GET for reset password confirmation
         * \return View for reset password
         */
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /*! \fn AccessDenied
         * \brief HTTP GET for access denied view
         * \return View for login page
         */
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        /*! \fn AddErrors
         * \brief Helper function to add errors to the model
         * \param result Identity result logs errors, and are added to the model
         */
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        /*! \fn RedirectToLocal
         * \brief Helper function to redirect locally
         * \param returnUrl Return URL
         * \return View based on returnUrl
         */
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}

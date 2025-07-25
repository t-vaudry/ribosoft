using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Models.AdminViewModels;

namespace Ribosoft.Controllers
{
    /*! \class AdminController
     * \brief Controller class for administrative user management
     */
    [Authorize(Roles = "Administrator")]
    [Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        /*! \property _userManager
         * \brief Local application user manager
         */
        private readonly UserManager<ApplicationUser> _userManager;

        /*! \property _roleManager
         * \brief Local application role manager
         */
        private readonly RoleManager<IdentityRole> _roleManager;

        /*! \property _context
         * \brief Database context
         */
        private readonly ApplicationDbContext _context;

        /*! \property _logger
         * \brief Local logging service
         */
        private readonly ILogger<AdminController> _logger;

        /*! \brief Constructor for AdminController
         * \param userManager User manager service
         * \param roleManager Role manager service
         * \param context Database context
         * \param logger Logging service
         */
        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        /*! \brief Display the admin dashboard with user management tabs
         * \return View with user list
         */
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "", int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.UserName.Contains(searchTerm) || 
                                           u.Email.Contains(searchTerm) ||
                                           (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                                           (u.LastName != null && u.LastName.Contains(searchTerm)));
                }

                var totalUsers = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.UserName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userViewModels = new List<UserListItemViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var jobCount = await _context.Jobs.CountAsync(j => j.Owner.Id == user.Id);

                    userViewModels.Add(new UserListItemViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        EmailConfirmed = user.EmailConfirmed,
                        LockoutEnd = user.LockoutEnd,
                        AccessFailedCount = user.AccessFailedCount,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        Roles = roles.ToList(),
                        JobCount = jobCount,
                        LastLoginDate = user.LastLoginDate,
                        RegistrationDate = user.RegistrationDate
                    });
                }

                // Apply role filter after loading roles
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    userViewModels = userViewModels.Where(u => u.Roles.Contains(roleFilter)).ToList();
                }

                var viewModel = new UserManagementViewModel
                {
                    Users = userViewModels,
                    SearchTerm = searchTerm,
                    RoleFilter = roleFilter,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalUsers = totalUsers,
                    TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize),
                    AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin user management page");
                TempData["ErrorMessage"] = "An error occurred while loading the user management page.";
                return RedirectToAction("Index", "Home");
            }
        }

        /*! \brief Get user details for viewing/editing
         * \param id User ID
         * \return JSON with user details
         */
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var jobCount = await _context.Jobs.CountAsync(j => j.Owner.Id == user.Id);
                var recentJobs = await _context.Jobs
                    .Where(j => j.Owner.Id == user.Id)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .Select(j => new { j.Id, j.Name, j.Status, j.CreatedAt })
                    .ToListAsync();

                var userDetails = new UserDetailsViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnd = user.LockoutEnd,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = roles.ToList(),
                    JobCount = jobCount,
                    LastLoginDate = user.LastLoginDate,
                    RegistrationDate = user.RegistrationDate,
                    RecentJobs = recentJobs
                };

                return Json(userDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for user {UserId}", id);
                return BadRequest("Error retrieving user details");
            }
        }

        /*! \brief Update user roles
         * \param model User role update model
         * \return JSON result
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles([FromBody] UpdateUserRolesViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Prevent removing Administrator role from the last admin
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains("Administrator") && !model.Roles.Contains("Administrator"))
                {
                    var adminCount = 0;
                    var allUsers = _userManager.Users.ToList();
                    foreach (var u in allUsers)
                    {
                        var userRoles = await _userManager.GetRolesAsync(u);
                        if (userRoles.Contains("Administrator"))
                        {
                            adminCount++;
                        }
                    }

                    if (adminCount <= 1)
                    {
                        return BadRequest("Cannot remove Administrator role from the last administrator");
                    }
                }

                // Remove all current roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest("Failed to remove current roles");
                }

                // Add new roles
                if (model.Roles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!addResult.Succeeded)
                    {
                        return BadRequest("Failed to add new roles");
                    }
                }

                _logger.LogInformation("User {UserId} roles updated by admin {AdminId}", user.Id, User.Identity.Name);
                return Json(new { success = true, message = "User roles updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles for user {UserId}", model.UserId);
                return BadRequest("Error updating user roles");
            }
        }

        /*! \brief Toggle user lockout status
         * \param id User ID
         * \return JSON result
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLockout(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Prevent locking out administrators
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains("Administrator"))
                {
                    return BadRequest("Cannot lock out administrator accounts");
                }

                IdentityResult result;
                string action;

                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    // User is currently locked out, unlock them
                    result = await _userManager.SetLockoutEndDateAsync(user, null);
                    action = "unlocked";
                }
                else
                {
                    // User is not locked out, lock them for 30 days
                    result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(30));
                    action = "locked";
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} {Action} by admin {AdminId}", user.Id, action, User.Identity.Name);
                    return Json(new { success = true, message = $"User {action} successfully", action = action });
                }
                else
                {
                    return BadRequest($"Failed to {action.Replace("ed", "")} user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling lockout for user {UserId}", id);
                return BadRequest("Error updating user lockout status");
            }
        }

        /*! \brief Reset user password
         * \param id User ID
         * \return JSON result
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Generate a random password
                var newPassword = GenerateRandomPassword();
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset for user {UserId} by admin {AdminId}", user.Id, User.Identity.Name);
                    return Json(new { success = true, message = "Password reset successfully", newPassword = newPassword });
                }
                else
                {
                    return BadRequest("Failed to reset password");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", id);
                return BadRequest("Error resetting user password");
            }
        }

        /*! \brief Generate a random password
         * \return Random password string
         */
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

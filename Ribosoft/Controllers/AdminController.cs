using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Models.AdminViewModels;
using Ribosoft.Services;
using Ribosoft.Extensions;

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

        /*! \property _environment
         * \brief Web host environment
         */
        private readonly IWebHostEnvironment _environment;

        /*! \property _activityLogService
         * \brief Activity log service
         */
        private readonly IActivityLogService _activityLogService;

        /*! \brief Constructor for AdminController
         * \param userManager User manager service
         * \param roleManager Role manager service
         * \param context Database context
         * \param logger Logging service
         * \param environment Web host environment
         * \param activityLogService Activity log service
         */
        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger,
            IWebHostEnvironment environment,
            IActivityLogService activityLogService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _environment = environment;
            _activityLogService = activityLogService;
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
                    query = query.Where(u => u.UserName!.Contains(searchTerm) || 
                                           u.Email!.Contains(searchTerm));
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
                    var jobCount = await _context.Jobs.CountAsync(j => j.OwnerId == user.Id);

                    userViewModels.Add(new UserListItemViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FirstName = "", // Not available in base ApplicationUser
                        LastName = "", // Not available in base ApplicationUser
                        EmailConfirmed = user.EmailConfirmed,
                        LockoutEnd = user.LockoutEnd,
                        AccessFailedCount = user.AccessFailedCount,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        Roles = roles.ToList(),
                        JobCount = jobCount,
                        LastLoginDate = null, // Not available in base ApplicationUser
                        RegistrationDate = DateTime.UtcNow // Use a default value
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
                    AvailableRoles = await _roleManager.Roles
                        .Where(r => r.Name != null)
                        .Select(r => r.Name!)
                        .ToListAsync()
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
                var jobCount = await _context.Jobs.CountAsync(j => j.OwnerId == user.Id);
                var recentJobs = await _context.Jobs
                    .Where(j => j.OwnerId == user.Id)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .Select(j => new { j.Id, JobName = j.RNAInput, State = j.JobState.ToString(), j.CreatedAt })
                    .ToListAsync();

                var userDetails = new UserDetailsViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = "", // Not available in base ApplicationUser
                    LastName = "", // Not available in base ApplicationUser
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber ?? "",
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnd = user.LockoutEnd,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    Roles = roles.ToList(),
                    JobCount = jobCount,
                    LastLoginDate = null, // Not available in base ApplicationUser
                    RegistrationDate = DateTime.UtcNow, // Use a default value
                    RecentJobs = null // Don't use this for the JSON endpoint
                };

                return Json(userDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for user {UserId}", SanitizeForLogging(id));
                return BadRequest("Error retrieving user details");
            }
        }

        /*! \fn GetUserDetailsModal
         * \brief Get user details modal content
         * \param id User ID
         * \return Partial view with user details
         */
        [HttpGet]
        public async Task<IActionResult> GetUserDetailsModal(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var jobCount = await _context.Jobs.CountAsync(j => j.OwnerId == user.Id);

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                TwoFactorEnabled = user.TwoFactorEnabled,
                AccessFailedCount = user.AccessFailedCount,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                Roles = roles.ToList(),
                JobCount = jobCount,
                RegistrationDate = DateTime.UtcNow, // Default since we don't track this
                RecentJobs = null // Not needed for this modal
            };

            return PartialView("_UserDetailsModal", viewModel);
        }

        /*! \fn GetEditRolesModal
         * \brief Get edit roles modal content
         * \param id User ID
         * \return Partial view with edit roles form
         */
        [HttpGet]
        public async Task<IActionResult> GetEditRolesModal(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new EditUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.UserName ?? string.Empty, // Use username as display name
                AvailableRoles = allRoles.Where(r => r != null).Cast<string>().ToList(),
                UserRoles = userRoles.ToList()
            };

            return PartialView("_EditRolesModal", viewModel);
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

                _logger.LogInformation("User {UserId} roles updated by admin {AdminId}", user.Id, User.Identity?.Name ?? "Unknown");
                return Json(new { success = true, message = "User roles updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles for user {UserId}", SanitizeForLogging(model.UserId));
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
                    _logger.LogInformation("User {UserId} {Action} by admin {AdminId}", user.Id, action, User.Identity?.Name ?? "Unknown");
                    
                    // Log admin activity
                    await _activityLogService.LogAdminActivityAsync(this, 
                        $"User account {action}", user.Id, user.UserName, 
                        action == "locked" ? "Warning" : "Information");
                    
                    return Json(new { success = true, message = $"User {action} successfully", action = action });
                }
                else
                {
                    return BadRequest($"Failed to {action.Replace("ed", "")} user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling lockout for user {UserId}", SanitizeForLogging(id));
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
                    return Json(new { success = false, message = "User not found" });
                }

                // Generate a random password
                var newPassword = GenerateRandomPassword();
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset for user {UserId} by admin {AdminId}", user.Id, User.Identity?.Name);
                    
                    // Log admin activity
                    await _activityLogService.LogAdminActivityAsync(this, 
                        $"Password reset for user", user.Id, user.UserName, "Warning");
                    
                    return Json(new { success = true, message = "Password reset successfully", newPassword = newPassword });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to reset password for user {UserId}: {Errors}", user.Id, errors);
                    return Json(new { success = false, message = $"Failed to reset password: {errors}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", SanitizeForLogging(id));
                return Json(new { success = false, message = "Error resetting user password" });
            }
        }

        /*! \brief Generate a random password
         * \return Random password string
         */
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[12];
            rng.GetBytes(bytes);
            
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }

        /*! \brief Sanitize user input for safe logging
         * \param input User input to sanitize
         * \return Sanitized string safe for logging
         */
        private static string SanitizeForLogging(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return "[empty]";
            
            // Remove or replace characters that could be used for log injection
            return input
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", " ")
                .Trim();
        }

        /*! \brief Display system information
         * \return System information view
         */
        [HttpGet]
        public async Task<IActionResult> SystemInfo()
        {
            try
            {
                var model = new SystemInfoViewModel();

                // Application Information
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                model.ApplicationVersion = assembly.GetName().Version?.ToString() ?? "Unknown";
                model.FrameworkVersion = Environment.Version.ToString();
                model.ApplicationStartTime = System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();

                // Server Information
                model.ServerName = Environment.MachineName;
                model.OperatingSystem = Environment.OSVersion.ToString();
                model.ProcessorCount = Environment.ProcessorCount.ToString();
                
                var process = System.Diagnostics.Process.GetCurrentProcess();
                model.WorkingSet = FormatBytes(process.WorkingSet64);
                model.TotalMemory = FormatBytes(GC.GetTotalMemory(false));

                // Database Information
                model.DatabaseProvider = _context.Database.ProviderName ?? "Unknown";
                try
                {
                    await _context.Database.OpenConnectionAsync();
                    model.DatabaseConnectionStatus = true;
                    model.DatabaseVersion = _context.Database.GetDbConnection().ServerVersion ?? "Unknown";
                    await _context.Database.CloseConnectionAsync();
                }
                catch
                {
                    model.DatabaseConnectionStatus = false;
                    model.DatabaseVersion = "Connection Failed";
                }

                // Database Statistics
                model.TotalUsers = await _context.Users.CountAsync();
                model.TotalJobs = await _context.Jobs.CountAsync();
                model.TotalRibozymes = await _context.Ribozymes.CountAsync();
                model.TotalDesigns = await _context.Designs.CountAsync();

                // Background Jobs Information (Hangfire)
                model.HangfireStatus = true; // Assume running if no errors
                // Note: Hangfire statistics would require Hangfire.Core references

                // Storage Information
                model.TempDirectory = Path.GetTempPath();
                model.TempDirectorySize = GetDirectorySize(Path.GetTempPath());
                
                // Configuration Information
                model.ConfigurationSettings = new Dictionary<string, string>
                {
                    ["Environment"] = _environment.EnvironmentName,
                    ["Content Root"] = _environment.ContentRootPath,
                    ["Web Root"] = _environment.WebRootPath
                };

                // Performance Metrics
                model.ThreadCount = process.Threads.Count;
                model.MemoryUsage = process.WorkingSet64;

                // Recent Activity
                var yesterday = DateTime.UtcNow.AddDays(-1);
                model.JobsLast24Hours = await _context.Jobs.CountAsync(j => j.CreatedAt >= yesterday);
                
                var lastJob = await _context.Jobs.OrderByDescending(j => j.CreatedAt).FirstOrDefaultAsync();
                model.LastJobSubmission = lastJob?.CreatedAt ?? DateTime.MinValue;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading system information");
                return View(new SystemInfoViewModel());
            }
        }

        /*! \brief Get logs data as JSON for AJAX loading
         * \return JSON with logs data and pagination info
         */
        [HttpGet]
        public async Task<IActionResult> LogsPartial(string? searchTerm, string[]? logLevels, string[]? categories, 
            DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 50)
        {
            try
            {
                // Set filters based on user selections - no defaults applied
                var selectedLogLevels = logLevels?.ToList() ?? new List<string>();
                var selectedCategories = categories?.ToList() ?? new List<string>();

                // Build query for activity logs
                var query = _context.ActivityLogs.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(l => l.Message.Contains(searchTerm) ||
                                           (l.UserName != null && l.UserName.Contains(searchTerm)) ||
                                           (l.Exception != null && l.Exception.Contains(searchTerm)));
                }

                if (selectedLogLevels.Any())
                {
                    query = query.Where(l => l.LogLevel != null && selectedLogLevels.Contains(l.LogLevel));
                }

                if (selectedCategories.Any())
                {
                    query = query.Where(l => l.Category != null && selectedCategories.Contains(l.Category));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var endOfDay = endDate.Value.Date.AddDays(1);
                    query = query.Where(l => l.Timestamp < endOfDay);
                }

                // Get total count for pagination
                var totalEntries = await query.CountAsync();

                // Get paginated results with related data
                var logEntries = await query
                    .Include(l => l.User)
                    .Include(l => l.Job)
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Transform to anonymous objects with explicit null handling
                var transformedEntries = logEntries.Select(l => new {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    LogLevel = l.LogLevel ?? "Unknown",
                    Category = l.Category ?? "General", 
                    Message = string.IsNullOrEmpty(l.Message) ? "No message available" : l.Message,
                    UserName = l.UserName ?? "System",
                    HasException = !string.IsNullOrEmpty(l.Exception),
                    LogLevelBadgeClass = l.LogLevelBadgeClass ?? "bg-secondary",
                    CategoryBadgeClass = l.CategoryBadgeClass(),
                    CategoryIcon = l.CategoryIcon()
                }).ToList();

                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalEntries / pageSize);

                return Json(new {
                    success = true,
                    data = transformedEntries,
                    pagination = new {
                        currentPage = page,
                        totalPages = totalPages,
                        totalEntries = totalEntries,
                        pageSize = pageSize,
                        hasPreviousPage = page > 1,
                        hasNextPage = page < totalPages
                    },
                    filters = new {
                        searchTerm = searchTerm,
                        selectedLogLevels = selectedLogLevels,
                        selectedCategories = selectedCategories,
                        startDate = startDate?.ToString("yyyy-MM-dd"),
                        endDate = endDate?.ToString("yyyy-MM-dd")
                    }
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activity logs partial");
                return Json(new { 
                    success = false, 
                    error = "Error loading logs. Please try again.",
                    data = new object[0],
                    pagination = new {
                        currentPage = 1,
                        totalPages = 0,
                        totalEntries = 0,
                        pageSize = pageSize,
                        hasPreviousPage = false,
                        hasNextPage = false
                    }
                });
            }
        }

        /*! \brief Display activity logs
         * \return Activity logs view
         */
        [HttpGet]
        public async Task<IActionResult> Logs(string? searchTerm, string[]? logLevels, string[]? categories, 
            DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 50)
        {
            try
            {
                // Set filters based on user selections - no defaults applied
                var selectedLogLevels = logLevels?.ToList() ?? new List<string>();
                var selectedCategories = categories?.ToList() ?? new List<string>();

                var model = new ActivityLogViewModel
                {
                    SearchTerm = searchTerm,
                    SelectedLogLevels = selectedLogLevels,
                    SelectedCategories = selectedCategories,
                    StartDate = startDate,
                    EndDate = endDate,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Build query for activity logs
                var query = _context.ActivityLogs.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(l => l.Message.Contains(searchTerm) ||
                                           (l.UserName != null && l.UserName.Contains(searchTerm)) ||
                                           (l.Exception != null && l.Exception.Contains(searchTerm)));
                }

                if (selectedLogLevels.Any())
                {
                    query = query.Where(l => l.LogLevel != null && selectedLogLevels.Contains(l.LogLevel));
                }

                if (selectedCategories.Any())
                {
                    query = query.Where(l => l.Category != null && selectedCategories.Contains(l.Category));
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var endOfDay = endDate.Value.Date.AddDays(1);
                    query = query.Where(l => l.Timestamp < endOfDay);
                }

                // Get total count for pagination
                model.TotalEntries = await query.CountAsync();

                // Get paginated results with related data
                var logs = await query
                    .Include(l => l.User)
                    .Include(l => l.Job)
                    .Include(l => l.Ribozyme)
                    .Include(l => l.Design)
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                model.LogEntries = logs;

                // Calculate statistics
                var yesterday = DateTime.UtcNow.AddDays(-1);
                var today = DateTime.UtcNow.Date;

                // Get all logs for statistics (limit to recent for performance)
                var recentLogs = await _context.ActivityLogs
                    .Where(l => l.Timestamp >= yesterday.AddDays(-7)) // Last week for stats
                    .ToListAsync();

                model.ErrorsLast24Hours = recentLogs.Count(l => l.Timestamp >= yesterday && l.LogLevel == "Error");
                model.WarningsLast24Hours = recentLogs.Count(l => l.Timestamp >= yesterday && l.LogLevel == "Warning");
                model.TotalEventsToday = recentLogs.Count(l => l.Timestamp >= today);

                // Log level counts
                model.LogLevelCounts = recentLogs.GroupBy(l => l.LogLevel)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Category counts (only non-null categories)
                model.CategoryCounts = recentLogs
                    .Where(l => !string.IsNullOrEmpty(l.Category))
                    .GroupBy(l => l.Category!)
                    .ToDictionary(g => g.Key, g => g.Count());

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activity logs");
                return View(new ActivityLogViewModel());
            }
        }

        /*! \brief Helper method to format bytes
         * \param bytes Number of bytes
         * \return Formatted string
         */
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        /*! \brief Get log details for modal display
         * \param id Log entry ID
         * \return Partial view with log details
         */
        [HttpGet]
        public async Task<IActionResult> LogDetails(int id)
        {
            try
            {
                _logger.LogInformation("Loading log details for ID: {LogId}", id);

                if (id <= 0)
                {
                    return BadRequest("Invalid log ID");
                }

                var logEntry = await _context.ActivityLogs
                    .Include(l => l.User)
                    .Include(l => l.Job)
                    .Include(l => l.Ribozyme)
                    .Include(l => l.Design)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (logEntry == null)
                {
                    _logger.LogWarning("Log entry not found for ID: {LogId}", id);
                    return NotFound($"Log entry with ID {id} not found");
                }

                _logger.LogInformation("Successfully loaded log entry {LogId}", id);
                return PartialView("_LogDetailsModal", logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading log details for ID {LogId}", id);
                return BadRequest($"Error loading log details: {ex.Message}");
            }
        }

        /*! \brief Helper method to get directory size
         * \param path Directory path
         * \return Formatted size string
         */
        private static string GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                long size = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                return FormatBytes(size);
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}

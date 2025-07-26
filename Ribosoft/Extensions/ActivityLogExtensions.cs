using Microsoft.AspNetCore.Mvc;
using Ribosoft.Services;
using Ribosoft.Models;
using System.Security.Claims;

namespace Ribosoft.Extensions
{
    /*! \class ActivityLogExtensions
     * \brief Extension methods for easier activity logging
     */
    public static class ActivityLogExtensions
    {
        /*! \brief Log user activity from controller context
         * \param activityLogService Activity log service
         * \param controller Controller instance
         * \param logLevel Log level
         * \param category Log category
         * \param message Log message
         * \param exception Exception details
         * \param jobId Associated job ID
         * \param ribozymeId Associated ribozyme ID
         * \param designId Associated design ID
         */
        public static async Task LogUserActivityAsync(this IActivityLogService activityLogService,
            Controller controller, string logLevel, string category, string message,
            string? exception = null, int? jobId = null, int? ribozymeId = null, int? designId = null)
        {
            var userId = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = controller.User.Identity?.Name;
            var ipAddress = controller.HttpContext.Connection.RemoteIpAddress?.ToString();
            var requestPath = controller.HttpContext.Request.Path;
            var requestMethod = controller.HttpContext.Request.Method;

            await activityLogService.LogAsync(
                logLevel: logLevel,
                category: category,
                message: message,
                exception: exception,
                userId: userId,
                userName: userName,
                ipAddress: ipAddress,
                requestPath: requestPath,
                requestMethod: requestMethod,
                jobId: jobId,
                ribozymeId: ribozymeId,
                designId: designId
            );
        }

        /*! \brief Log authentication activity from controller context
         * \param activityLogService Activity log service
         * \param controller Controller instance
         * \param message Log message
         * \param success Whether authentication was successful
         * \param targetUserId Target user ID (for admin actions)
         * \param targetUserName Target user name (for admin actions)
         */
        public static async Task LogAuthenticationActivityAsync(this IActivityLogService activityLogService,
            Controller controller, string message, bool success = true, 
            string? targetUserId = null, string? targetUserName = null)
        {
            var userId = targetUserId ?? controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = targetUserName ?? controller.User.Identity?.Name;
            var ipAddress = controller.HttpContext.Connection.RemoteIpAddress?.ToString();

            await activityLogService.LogAuthenticationAsync(
                message: message,
                userId: userId,
                userName: userName,
                ipAddress: ipAddress,
                success: success
            );
        }

        /*! \brief Log job-related activity from controller context
         * \param activityLogService Activity log service
         * \param controller Controller instance
         * \param message Log message
         * \param jobId Job ID
         * \param logLevel Log level
         */
        public static async Task LogJobActivityAsync(this IActivityLogService activityLogService,
            Controller controller, string message, int jobId, string logLevel = "Information")
        {
            var userId = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = controller.User.Identity?.Name;

            await activityLogService.LogJobExecutionAsync(
                message: message,
                jobId: jobId,
                userId: userId,
                userName: userName,
                logLevel: logLevel
            );
        }

        /*! \brief Log ribozyme design activity from controller context
         * \param activityLogService Activity log service
         * \param controller Controller instance
         * \param message Log message
         * \param ribozymeId Ribozyme ID
         * \param logLevel Log level
         */
        public static async Task LogRibozymeActivityAsync(this IActivityLogService activityLogService,
            Controller controller, string message, int ribozymeId, string logLevel = "Information")
        {
            var userId = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = controller.User.Identity?.Name;

            await activityLogService.LogRibozymeDesignAsync(
                message: message,
                ribozymeId: ribozymeId,
                userId: userId,
                userName: userName,
                logLevel: logLevel
            );
        }

        /*! \brief Log admin activity from controller context
         * \param activityLogService Activity log service
         * \param controller Controller instance
         * \param message Log message
         * \param targetUserId Target user ID
         * \param targetUserName Target user name
         * \param logLevel Log level
         */
        public static async Task LogAdminActivityAsync(this IActivityLogService activityLogService,
            Controller controller, string message, string? targetUserId = null, 
            string? targetUserName = null, string logLevel = "Information")
        {
            var adminUserId = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUserName = controller.User.Identity?.Name;
            var ipAddress = controller.HttpContext.Connection.RemoteIpAddress?.ToString();

            var fullMessage = targetUserName != null 
                ? $"{message} (Target: {targetUserName})"
                : message;

            await activityLogService.LogAsync(
                logLevel: logLevel,
                category: "UserManagement",
                message: fullMessage,
                userId: adminUserId,
                userName: adminUserName,
                ipAddress: ipAddress,
                requestPath: controller.HttpContext.Request.Path,
                requestMethod: controller.HttpContext.Request.Method
            );
        }
    }

    /*! \class ActivityLogDisplayExtensions
     * \brief Extension methods for ActivityLog display properties
     */
    public static class ActivityLogDisplayExtensions
    {
        /*! \brief Get CSS class for log level badge */
        public static string LogLevelBadgeClass(this ActivityLog log)
        {
            return log.LogLevel?.ToLower() switch
            {
                "error" => "bg-danger",
                "warning" => "bg-warning text-dark",
                "information" => "bg-info",
                "debug" => "bg-secondary",
                "trace" => "bg-light text-dark",
                _ => "bg-primary"
            };
        }

        /*! \brief Get CSS class for category badge */
        public static string CategoryBadgeClass(this ActivityLog log)
        {
            var category = log.Category?.ToLower();
            return category switch
            {
                "authentication" => "bg-success",
                "usermanagement" or "user management" => "bg-warning text-dark",
                "jobsubmission" or "job submission" => "bg-primary",
                "jobexecution" or "job execution" => "bg-info",
                "ribozymedesign" or "ribozyme design" => "bg-success",
                "httprequest" or "http request" => "bg-secondary",
                "error" => "bg-danger",
                _ => "bg-light text-dark border"
            };
        }

        /*! \brief Get FontAwesome icon for category */
        public static string CategoryIcon(this ActivityLog log)
        {
            var category = log.Category?.ToLower();
            return category switch
            {
                "authentication" => "fas fa-sign-in-alt",
                "usermanagement" or "user management" => "fas fa-users-cog",
                "jobsubmission" or "job submission" => "fas fa-paper-plane",
                "jobexecution" or "job execution" => "fas fa-cogs",
                "ribozymedesign" or "ribozyme design" => "fas fa-dna",
                "httprequest" or "http request" => "fas fa-globe",
                "error" => "fas fa-exclamation-triangle",
                _ => "fas fa-info-circle"
            };
        }

        /*! \brief Check if log has user information */
        public static bool HasUserInfo(this ActivityLog log)
        {
            return !string.IsNullOrEmpty(log.UserId) || 
                   !string.IsNullOrEmpty(log.UserName) || 
                   !string.IsNullOrEmpty(log.IpAddress) ||
                   !string.IsNullOrEmpty(log.UserAgent);
        }

        /*! \brief Check if log has request information */
        public static bool HasRequestInfo(this ActivityLog log)
        {
            return !string.IsNullOrEmpty(log.RequestPath) || 
                   !string.IsNullOrEmpty(log.RequestMethod) || 
                   log.StatusCode.HasValue || 
                   log.Duration.HasValue;
        }

        /*! \brief Check if log has job information */
        public static bool HasJobInfo(this ActivityLog log)
        {
            return log.JobId.HasValue;
        }

        /*! \brief Check if log has ribozyme information */
        public static bool HasRibozymeInfo(this ActivityLog log)
        {
            return log.RibozymeId.HasValue;
        }

        /*! \brief Check if log has design information */
        public static bool HasDesignInfo(this ActivityLog log)
        {
            return log.DesignId.HasValue;
        }

        /*! \brief Check if log has exception information */
        public static bool HasException(this ActivityLog log)
        {
            return !string.IsNullOrEmpty(log.Exception);
        }
    }
}

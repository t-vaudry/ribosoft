using Ribosoft.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace Ribosoft.Middleware
{
    /*! \class RequestLoggingMiddleware
     * \brief Middleware to log HTTP requests to the activity log
     */
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /*! \brief Constructor for RequestLoggingMiddleware
         * \param next Next middleware in pipeline
         * \param logger Logger instance
         */
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /*! \brief Invoke middleware
         * \param context HTTP context
         * \param activityLogService Activity log service
         */
        public async Task InvokeAsync(HttpContext context, IActivityLogService activityLogService)
        {
            // Skip logging for static files and health checks
            if (ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log the request
                await LogRequestAsync(context, activityLogService, stopwatch.ElapsedMilliseconds);
            }
        }

        /*! \brief Determine if request should be skipped from logging
         * \param path Request path
         * \return True if should skip logging
         */
        private static bool ShouldSkipLogging(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? "";
            
            return pathValue.StartsWith("/css/") ||
                   pathValue.StartsWith("/js/") ||
                   pathValue.StartsWith("/images/") ||
                   pathValue.StartsWith("/lib/") ||
                   pathValue.StartsWith("/favicon.ico") ||
                   pathValue.StartsWith("/health") ||
                   pathValue.StartsWith("/hangfire/") ||
                   pathValue.Contains(".map") ||
                   pathValue.Contains(".woff") ||
                   pathValue.Contains(".ttf") ||
                   pathValue.Contains(".eot");
        }

        /*! \brief Log the HTTP request
         * \param context HTTP context
         * \param activityLogService Activity log service
         * \param duration Request duration in milliseconds
         */
        private async Task LogRequestAsync(HttpContext context, IActivityLogService activityLogService, long duration)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                var user = context.User;

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity?.Name;
                var ipAddress = GetClientIpAddress(context);

                var logLevel = response.StatusCode >= 500 ? "Error" :
                              response.StatusCode >= 400 ? "Warning" : "Information";

                var category = DetermineCategory(request.Path);
                var message = $"{request.Method} {request.Path} - {response.StatusCode}";

                await activityLogService.LogAsync(
                    logLevel: logLevel,
                    category: category,
                    message: message,
                    userId: userId,
                    userName: userName,
                    ipAddress: ipAddress,
                    requestPath: request.Path,
                    requestMethod: request.Method,
                    statusCode: response.StatusCode,
                    duration: duration
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log HTTP request");
            }
        }

        /*! \brief Get client IP address
         * \param context HTTP context
         * \return Client IP address
         */
        private static string? GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first (for load balancers/proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        /*! \brief Determine log category based on request path
         * \param path Request path
         * \return Log category
         */
        private static string DetermineCategory(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? "";

            return pathValue switch
            {
                var p when p.StartsWith("/account/") => "Authentication",
                var p when p.StartsWith("/admin/") => "UserManagement",
                var p when p.StartsWith("/jobs/") => "JobExecution",
                var p when p.StartsWith("/request/") => "JobSubmission",
                var p when p.StartsWith("/ribozymes/") => "RibozymeDesign",
                var p when p.StartsWith("/designs/") => "RibozymeDesign",
                var p when p.StartsWith("/api/") => "API",
                var p when p.StartsWith("/manage/") => "UserManagement",
                _ => "System"
            };
        }
    }

    /*! \class RequestLoggingMiddlewareExtensions
     * \brief Extension methods for RequestLoggingMiddleware
     */
    public static class RequestLoggingMiddlewareExtensions
    {
        /*! \brief Add request logging middleware to pipeline
         * \param builder Application builder
         * \return Application builder
         */
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}

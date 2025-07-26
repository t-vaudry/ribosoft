using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;
using System.Text.Json;

namespace Ribosoft.Services
{
    /*! \interface IActivityLogService
     * \brief Interface for activity logging service
     */
    public interface IActivityLogService
    {
        Task LogAsync(string logLevel, string category, string message, string? exception = null, 
                     string? userId = null, string? userName = null, string? ipAddress = null,
                     string? requestPath = null, string? requestMethod = null, int? statusCode = null,
                     long? duration = null, int? jobId = null, int? ribozymeId = null, int? designId = null,
                     Dictionary<string, object>? properties = null);

        Task LogAuthenticationAsync(string message, string? userId = null, string? userName = null, 
                                   string? ipAddress = null, bool success = true);

        Task LogJobExecutionAsync(string message, int jobId, string? userId = null, 
                                 string? userName = null, string logLevel = "Information");

        Task LogRibozymeDesignAsync(string message, int ribozymeId, string? userId = null, 
                                   string? userName = null, string logLevel = "Information");

        Task LogSystemEventAsync(string message, string logLevel = "Information");

        Task LogErrorAsync(string message, Exception? exception = null, string? userId = null, 
                          string? userName = null, string? requestPath = null);
    }

    /*! \class ActivityLogService
     * \brief Service for logging application activities to database
     */
    public class ActivityLogService : IActivityLogService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActivityLogService> _logger;

        /*! \brief Constructor for ActivityLogService
         * \param serviceProvider Service provider for dependency resolution
         * \param logger Logger instance
         */
        public ActivityLogService(IServiceProvider serviceProvider, ILogger<ActivityLogService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /*! \brief Log a general activity
         * \param logLevel Log level
         * \param category Log category
         * \param message Log message
         * \param exception Exception details
         * \param userId User ID
         * \param userName User name
         * \param ipAddress IP address
         * \param requestPath Request path
         * \param requestMethod Request method
         * \param statusCode HTTP status code
         * \param duration Request duration
         * \param jobId Associated job ID
         * \param ribozymeId Associated ribozyme ID
         * \param designId Associated design ID
         * \param properties Additional properties
         */
        public async Task LogAsync(string logLevel, string category, string message, string? exception = null,
                                  string? userId = null, string? userName = null, string? ipAddress = null,
                                  string? requestPath = null, string? requestMethod = null, int? statusCode = null,
                                  long? duration = null, int? jobId = null, int? ribozymeId = null, int? designId = null,
                                  Dictionary<string, object>? properties = null)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var activityLog = new ActivityLog
                {
                    Timestamp = DateTime.UtcNow,
                    LogLevel = logLevel,
                    Category = category,
                    Message = message,
                    Exception = exception,
                    UserId = userId,
                    UserName = userName,
                    IpAddress = ipAddress,
                    RequestPath = requestPath,
                    RequestMethod = requestMethod,
                    StatusCode = statusCode,
                    Duration = duration,
                    JobId = jobId,
                    RibozymeId = ribozymeId,
                    DesignId = designId,
                    Properties = properties != null ? JsonSerializer.Serialize(properties) : null
                };

                context.ActivityLogs.Add(activityLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Don't let logging errors break the application
                _logger.LogError(ex, "Failed to write activity log to database");
            }
        }

        /*! \brief Log authentication events
         * \param message Log message
         * \param userId User ID
         * \param userName User name
         * \param ipAddress IP address
         * \param success Whether authentication was successful
         */
        public async Task LogAuthenticationAsync(string message, string? userId = null, string? userName = null,
                                               string? ipAddress = null, bool success = true)
        {
            await LogAsync(
                logLevel: success ? "Information" : "Warning",
                category: "Authentication",
                message: message,
                userId: userId,
                userName: userName,
                ipAddress: ipAddress
            );
        }

        /*! \brief Log job execution events
         * \param message Log message
         * \param jobId Job ID
         * \param userId User ID
         * \param userName User name
         * \param logLevel Log level
         */
        public async Task LogJobExecutionAsync(string message, int jobId, string? userId = null,
                                             string? userName = null, string logLevel = "Information")
        {
            await LogAsync(
                logLevel: logLevel,
                category: "JobExecution",
                message: message,
                userId: userId,
                userName: userName,
                jobId: jobId
            );
        }

        /*! \brief Log ribozyme design events
         * \param message Log message
         * \param ribozymeId Ribozyme ID
         * \param userId User ID
         * \param userName User name
         * \param logLevel Log level
         */
        public async Task LogRibozymeDesignAsync(string message, int ribozymeId, string? userId = null,
                                               string? userName = null, string logLevel = "Information")
        {
            await LogAsync(
                logLevel: logLevel,
                category: "RibozymeDesign",
                message: message,
                userId: userId,
                userName: userName,
                ribozymeId: ribozymeId
            );
        }

        /*! \brief Log system events
         * \param message Log message
         * \param logLevel Log level
         */
        public async Task LogSystemEventAsync(string message, string logLevel = "Information")
        {
            await LogAsync(
                logLevel: logLevel,
                category: "System",
                message: message
            );
        }

        /*! \brief Log error events
         * \param message Log message
         * \param exception Exception details
         * \param userId User ID
         * \param userName User name
         * \param requestPath Request path
         */
        public async Task LogErrorAsync(string message, Exception? exception = null, string? userId = null,
                                      string? userName = null, string? requestPath = null)
        {
            await LogAsync(
                logLevel: "Error",
                category: "System",
                message: message,
                exception: exception?.ToString(),
                userId: userId,
                userName: userName,
                requestPath: requestPath
            );
        }
    }
}

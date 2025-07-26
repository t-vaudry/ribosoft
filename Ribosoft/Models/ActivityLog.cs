using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ribosoft.Models
{
    /*! \class ActivityLog
     * \brief Entity model for storing application activity logs
     */
    public class ActivityLog : BaseEntity
    {
        /*! \property Id
         * \brief Primary key for the activity log entry
         */
        [Key]
        public int Id { get; set; }

        /*! \property Timestamp
         * \brief When the log entry was created
         */
        [Required]
        public DateTime Timestamp { get; set; }

        /*! \property LogLevel
         * \brief Log level (Trace, Debug, Information, Warning, Error, Critical)
         */
        [Required]
        [MaxLength(50)]
        public string LogLevel { get; set; } = string.Empty;

        /*! \property Category
         * \brief Log category (Authentication, JobExecution, etc.)
         */
        [MaxLength(100)]
        public string? Category { get; set; }

        /*! \property Message
         * \brief Log message content
         */
        [Required]
        public string Message { get; set; } = string.Empty;

        /*! \property Exception
         * \brief Exception details if applicable
         */
        public string? Exception { get; set; }

        /*! \property UserId
         * \brief ID of the user associated with this log entry
         */
        [MaxLength(450)]
        public string? UserId { get; set; }

        /*! \property UserName
         * \brief Username associated with this log entry
         */
        [MaxLength(256)]
        public string? UserName { get; set; }

        /*! \property IpAddress
         * \brief IP address of the request
         */
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /*! \property UserAgent
         * \brief User agent string from the request
         */
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        /*! \property RequestPath
         * \brief Request path/URL
         */
        [MaxLength(500)]
        public string? RequestPath { get; set; }

        /*! \property RequestMethod
         * \brief HTTP request method (GET, POST, etc.)
         */
        [MaxLength(10)]
        public string? RequestMethod { get; set; }

        /*! \property StatusCode
         * \brief HTTP status code
         */
        public int? StatusCode { get; set; }

        /*! \property Duration
         * \brief Request duration in milliseconds
         */
        public long? Duration { get; set; }

        /*! \property JobId
         * \brief Associated job ID if applicable
         */
        public int? JobId { get; set; }

        /*! \property RibozymeId
         * \brief Associated ribozyme ID if applicable
         */
        public int? RibozymeId { get; set; }

        /*! \property DesignId
         * \brief Associated design ID if applicable
         */
        public int? DesignId { get; set; }

        /*! \property Properties
         * \brief Additional properties as JSON
         */
        public string? Properties { get; set; }

        // Navigation properties
        /*! \property User
         * \brief Navigation property to the associated user
         */
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        /*! \property Job
         * \brief Navigation property to the associated job
         */
        [ForeignKey("JobId")]
        public virtual Job? Job { get; set; }

        /*! \property Ribozyme
         * \brief Navigation property to the associated ribozyme
         */
        [ForeignKey("RibozymeId")]
        public virtual Ribozyme? Ribozyme { get; set; }

        /*! \property Design
         * \brief Navigation property to the associated design
         */
        [ForeignKey("DesignId")]
        public virtual Design? Design { get; set; }

        // Helper properties for UI display
        /*! \property LogLevelBadgeClass
         * \brief CSS class for log level badge
         */
        [NotMapped]
        public string LogLevelBadgeClass => LogLevel.ToLower() switch
        {
            "trace" => "bg-secondary",
            "debug" => "bg-info",
            "information" => "bg-primary",
            "warning" => "bg-warning text-dark",
            "error" => "bg-danger",
            "critical" => "bg-dark",
            _ => "bg-secondary"
        };

        /*! \property CategoryBadgeClass
         * \brief CSS class for category badge
         */
        [NotMapped]
        public string CategoryBadgeClass => (Category?.ToLower()) switch
        {
            "authentication" => "bg-success",
            "authorization" => "bg-warning text-dark",
            "usermanagement" => "bg-info",
            "jobexecution" => "bg-primary",
            "jobsubmission" => "bg-primary",
            "ribozymedesign" => "bg-success",
            "sequenceanalysis" => "bg-info",
            "blastsearch" => "bg-warning text-dark",
            "database" => "bg-secondary",
            "system" => "bg-dark",
            "security" => "bg-danger",
            "performance" => "bg-warning text-dark",
            "api" => "bg-info",
            "backgroundjobs" => "bg-primary",
            _ => "bg-light text-dark"
        };

        /*! \property CategoryIcon
         * \brief Font Awesome icon for category
         */
        [NotMapped]
        public string CategoryIcon => (Category?.ToLower()) switch
        {
            "authentication" => "fas fa-sign-in-alt",
            "authorization" => "fas fa-shield-alt",
            "usermanagement" => "fas fa-users",
            "jobexecution" => "fas fa-cogs",
            "jobsubmission" => "fas fa-upload",
            "ribozymedesign" => "fas fa-dna",
            "sequenceanalysis" => "fas fa-search",
            "blastsearch" => "fas fa-search-plus",
            "database" => "fas fa-database",
            "system" => "fas fa-server",
            "security" => "fas fa-lock",
            "performance" => "fas fa-tachometer-alt",
            "api" => "fas fa-code",
            "backgroundjobs" => "fas fa-tasks",
            _ => "fas fa-info-circle"
        };

        /*! \property HasException
         * \brief Whether this log entry has exception details
         */
        [NotMapped]
        public bool HasException => !string.IsNullOrEmpty(Exception);

        /*! \property HasUserInfo
         * \brief Whether this log entry has user information
         */
        [NotMapped]
        public bool HasUserInfo => !string.IsNullOrEmpty(UserId);

        /*! \property HasRequestInfo
         * \brief Whether this log entry has request information
         */
        [NotMapped]
        public bool HasRequestInfo => !string.IsNullOrEmpty(RequestPath);

        /*! \property HasJobInfo
         * \brief Whether this log entry is associated with a job
         */
        [NotMapped]
        public bool HasJobInfo => JobId.HasValue;

        /*! \property HasRibozymeInfo
         * \brief Whether this log entry is associated with a ribozyme
         */
        [NotMapped]
        public bool HasRibozymeInfo => RibozymeId.HasValue;

        /*! \property HasDesignInfo
         * \brief Whether this log entry is associated with a design
         */
        [NotMapped]
        public bool HasDesignInfo => DesignId.HasValue;
    }
}

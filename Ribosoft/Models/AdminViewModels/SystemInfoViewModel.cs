using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.AdminViewModels
{
    public class SystemInfoViewModel
    {
        // Application Information
        public string ApplicationName { get; set; } = "Ribosoft";
        public string ApplicationVersion { get; set; } = string.Empty;
        public string FrameworkVersion { get; set; } = string.Empty;
        public DateTime ApplicationStartTime { get; set; }
        public TimeSpan Uptime => DateTime.UtcNow - ApplicationStartTime;

        // Server Information
        public string ServerName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string ProcessorCount { get; set; } = string.Empty;
        public string TotalMemory { get; set; } = string.Empty;
        public string AvailableMemory { get; set; } = string.Empty;
        public string WorkingSet { get; set; } = string.Empty;

        // Database Information
        public string DatabaseProvider { get; set; } = string.Empty;
        public string DatabaseVersion { get; set; } = string.Empty;
        public bool DatabaseConnectionStatus { get; set; }
        public string DatabaseConnectionString { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalRibozymes { get; set; }
        public int TotalDesigns { get; set; }

        // Background Jobs Information
        public bool HangfireStatus { get; set; }
        public int ActiveJobs { get; set; }
        public int ScheduledJobs { get; set; }
        public int FailedJobs { get; set; }
        public int SucceededJobs { get; set; }

        // Storage Information
        public string TempDirectory { get; set; } = string.Empty;
        public string TempDirectorySize { get; set; } = string.Empty;
        public string LogDirectory { get; set; } = string.Empty;
        public string LogDirectorySize { get; set; } = string.Empty;

        // Configuration Information
        public Dictionary<string, string> ConfigurationSettings { get; set; } = new();
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

        // Performance Metrics
        public double CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        // Recent Activity
        public DateTime LastUserLogin { get; set; }
        public DateTime LastJobSubmission { get; set; }
        public int JobsLast24Hours { get; set; }
        public int UsersLast24Hours { get; set; }
    }
}

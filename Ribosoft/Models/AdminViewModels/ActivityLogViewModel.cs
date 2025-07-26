using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.AdminViewModels
{
    public class ActivityLogViewModel
    {
        public List<ActivityLog> LogEntries { get; set; } = new();
        public int TotalEntries { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages => (int)Math.Ceiling((double)TotalEntries / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        // Filters - now support multiple selections
        public string? SearchTerm { get; set; }
        public List<string> SelectedLogLevels { get; set; } = new();
        public List<string> SelectedCategories { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Legacy single-select properties for backward compatibility
        public string? LogLevel => SelectedLogLevels.FirstOrDefault();
        public string? Category => SelectedCategories.FirstOrDefault();

        // Available filter options
        public List<string> AvailableLogLevels { get; set; } = new() 
        { 
            "Trace", "Debug", "Information", "Warning", "Error", "Critical" 
        };
        
        public List<string> AvailableCategories { get; set; } = new()
        {
            "Authentication", "Authorization", "UserManagement", "JobExecution", "JobSubmission",
            "RibozymeDesign", "SequenceAnalysis", "BlastSearch", "Database", "System", 
            "Security", "Performance", "API", "BackgroundJobs"
        };

        // Helper methods
        public bool HasActiveFilters => 
            !string.IsNullOrEmpty(SearchTerm) || 
            SelectedLogLevels.Any() || 
            SelectedCategories.Any() || 
            StartDate.HasValue || 
            EndDate.HasValue;

        // Summary statistics
        public Dictionary<string, int> LogLevelCounts { get; set; } = new();
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
        public int ErrorsLast24Hours { get; set; }
        public int WarningsLast24Hours { get; set; }
        public int TotalEventsToday { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ribosoft.Data;
using Ribosoft.Jobs;
using Ribosoft.Models;
using Ribosoft.Models.ViewModels;
using Ribosoft.Services;

namespace Ribosoft.Controllers
{
    /*! \class AssembliesController
     * \brief Controller class for the assemblies in the application
     */
    [Authorize(Roles = "Administrator")]
    public class AssembliesController : Controller
    {
        /*! \property _context
         * \brief Local database context
         */
        private readonly ApplicationDbContext _context;

        /*! \property _configuration
         * \brief Local application configuration
         */
        private readonly IConfiguration _configuration;

        /*! \property _logger
         * \brief Logger instance
         */
        private readonly ILogger<AssembliesController> _logger;

        /*! \property _datasetDownloadService
         * \brief Dataset download service
         */
        private readonly IDatasetDownloadService _datasetDownloadService;

        /*! \fn AssembliesController
         * \brief Default constructor
         * \param context Database context information
         * \param configuration Configuration of the application
         * \param logger Logger instance
         * \param datasetDownloadService Dataset download service
         */
        public AssembliesController(
            ApplicationDbContext context, 
            IConfiguration configuration, 
            ILogger<AssembliesController> logger,
            IDatasetDownloadService datasetDownloadService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _datasetDownloadService = datasetDownloadService;
        }

        /*! \fn Index
         * \brief HTTP GET assemblies index
         * \return View of assemblies index
         */
        public async Task<IActionResult> Index()
        {
            ViewBag.BlastDbPath = _configuration["Blast:BLASTDB"] ?? "Not configured";
            
            // Get current assemblies and recent downloads
            var assemblies = await _context.Assemblies.ToListAsync();
            var recentDownloads = await _context.DatasetDownloads
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Process assemblies to fix missing data and calculate sizes
            foreach (var assembly in assemblies)
            {
                // Fix missing Type - determine actual assembly type
                if (string.IsNullOrEmpty(assembly.Type))
                {
                    assembly.Type = DetermineAssemblyType(assembly.Path, assembly.AccessionId);
                }

                // Fix missing or incorrect Path
                if (string.IsNullOrEmpty(assembly.Path) || assembly.Path.StartsWith("~/"))
                {
                    var blastDbPath = _configuration["Blast:BLASTDB"] ?? 
                                     Path.Combine(Directory.GetCurrentDirectory(), "BlastDatabases");
                    blastDbPath = ExpandPath(blastDbPath);
                    
                    // Use organized path structure
                    assembly.Path = Path.Combine(blastDbPath, assembly.AccessionId);
                }

                // Calculate size
                assembly.Size = CalculateAssemblySize(assembly.Path);
            }

            ViewBag.RecentDownloads = recentDownloads;
            
            return View(assemblies);
        }

        /*! \fn BrowseDatasets
         * \brief HTTP GET for browsing available NCBI datasets
         * \param searchTerm Optional search term
         * \param limit Maximum results to return
         * \return View with available datasets
         */
        public async Task<IActionResult> BrowseDatasets(string? searchTerm = null, int limit = 50)
        {
            try
            {
                _logger.LogInformation("BrowseDatasets called with searchTerm: {SearchTerm}, limit: {Limit}", searchTerm, limit);
                
                var datasets = await _datasetDownloadService.GetAvailableDatasetsAsync(searchTerm, limit);
                
                _logger.LogInformation("Retrieved {Count} datasets from service", datasets.Count);
                
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Limit = limit;
                
                return View(datasets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing datasets with search term: {SearchTerm}", searchTerm);
                TempData["Error"] = "Error loading available datasets. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        /*! \fn SearchDatasets
         * \brief AJAX endpoint for searching datasets without page refresh
         * \param searchTerm Optional search term
         * \param limit Maximum results to return
         * \return JSON response with datasets
         */
        [HttpPost]
        public async Task<IActionResult> SearchDatasets([FromBody] SearchDatasetsRequest request)
        {
            try
            {
                _logger.LogInformation("AJAX SearchDatasets called with searchTerm: {SearchTerm}, limit: {Limit}", 
                    request.SearchTerm, request.Limit);
                
                var datasets = await _datasetDownloadService.GetAvailableDatasetsAsync(request.SearchTerm, request.Limit);
                
                _logger.LogInformation("Retrieved {Count} datasets from service via AJAX", datasets.Count);
                
                return Json(new { 
                    success = true, 
                    datasets = datasets,
                    searchTerm = request.SearchTerm,
                    count = datasets.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AJAX search with term: {SearchTerm}", request.SearchTerm);
                return Json(new { 
                    success = false, 
                    error = "Error loading datasets. Please try again later.",
                    datasets = new List<object>(),
                    count = 0
                });
            }
        }

        /*! \class SearchDatasetsRequest
         * \brief Request model for AJAX dataset search
         */
        public class SearchDatasetsRequest
        {
            public string? SearchTerm { get; set; }
            public int Limit { get; set; } = 50;
        }

        /*! \fn DownloadDatasets
         * \brief HTTP POST for requesting dataset downloads
         * \param request Download request
         * \return Redirect to downloads page
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadDatasets(DatasetDownloadRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid download request.";
                return RedirectToAction(nameof(BrowseDatasets));
            }

            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                var jobIds = await _datasetDownloadService.RequestDownloadAsync(request, userName);
                
                _logger.LogInformation("User {User} requested download of {Count} datasets. Job IDs: {JobIds}", 
                    userName, request.AccessionIds.Count, jobIds);
                
                TempData["Success"] = $"Download request submitted for {request.AccessionIds.Count} dataset(s). " +
                                     "You can monitor progress on the Downloads page.";
                
                return RedirectToAction(nameof(Downloads));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing download request for user {User}", User.Identity?.Name);
                TempData["Error"] = "Error processing download request. Please try again.";
                return RedirectToAction(nameof(BrowseDatasets));
            }
        }

        /*! \fn Downloads
         * \brief HTTP GET for viewing download history and status with pagination
         * \param page Current page number
         * \return View with paginated download history
         */
        public async Task<IActionResult> Downloads(int page = 1)
        {
            try
            {
                const int pageSize = 10;
                var downloads = await _datasetDownloadService.GetDownloadHistoryAsync(page, pageSize);
                var totalCount = await _datasetDownloadService.GetDownloadCountAsync();
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                return View(downloads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading download history");
                TempData["Error"] = "Error loading download history.";
                return RedirectToAction(nameof(Index));
            }
        }

        /*! \fn GetDownloadsPage
         * \brief HTTP GET for getting paginated downloads via AJAX
         * \param page Current page number
         * \return Partial view with downloads
         */
        [HttpGet]
        public async Task<IActionResult> GetDownloadsPage(int page = 1)
        {
            try
            {
                const int pageSize = 10;
                var downloads = await _datasetDownloadService.GetDownloadHistoryAsync(page, pageSize);
                var totalCount = await _datasetDownloadService.GetDownloadCountAsync();
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                return PartialView("_DownloadsTable", downloads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading downloads page {Page}", page);
                return Json(new { success = false, message = "Error loading downloads" });
            }
        }

        /*! \fn GetDownloadStatus
         * \brief HTTP GET for getting download status (AJAX)
         * \param id Download ID
         * \return JSON with download status
         */
        [HttpGet]
        public async Task<IActionResult> GetDownloadStatus(int id)
        {
            try
            {
                var download = await _datasetDownloadService.GetDownloadStatusAsync(id);
                if (download == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    id = download.Id,
                    accessionId = download.AccessionId,
                    status = download.Status.ToString(),
                    progress = download.Progress,
                    errorMessage = download.ErrorMessage,
                    startedAt = download.StartedAt,
                    completedAt = download.CompletedAt,
                    fileSize = download.FileSize,
                    downloadedBytes = download.DownloadedBytes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting download status for ID {DownloadId}", id);
                return StatusCode(500, new { error = "Error retrieving download status" });
            }
        }

        /*! \fn CancelDownload
         * \brief HTTP POST for cancelling a download
         * \param id Download ID
         * \return JSON result
         */
        [HttpPost]
        public async Task<IActionResult> CancelDownload(int id)
        {
            try
            {
                await _datasetDownloadService.CancelDownloadAsync(id);
                
                _logger.LogInformation("User {User} cancelled download {DownloadId}", User.Identity?.Name, id);
                
                return Json(new { success = true, message = "Download cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling download {DownloadId}", id);
                return Json(new { success = false, message = "Error cancelling download" });
            }
        }

        /*! \fn RetryDownload
         * \brief HTTP POST for retrying a failed download
         * \param id Download ID
         * \return JSON result
         */
        [HttpPost]
        public async Task<IActionResult> RetryDownload(int id)
        {
            try
            {
                await _datasetDownloadService.RetryDownloadAsync(id);
                
                _logger.LogInformation("User {User} retried download {DownloadId}", User.Identity?.Name, id);
                
                return Json(new { success = true, message = "Download retry initiated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying download {DownloadId}", id);
                return Json(new { success = false, message = "Error retrying download" });
            }
        }

        /*! \fn UpdateUnknownOrganisms
         * \brief HTTP POST for updating existing records with unknown organisms
         * \return JSON result with update count
         */
        [HttpPost]
        public async Task<IActionResult> UpdateUnknownOrganisms()
        {
            try
            {
                var updatedCount = await _datasetDownloadService.UpdateUnknownOrganismsAsync();
                
                _logger.LogInformation("User {User} updated {Count} unknown organism records", 
                    User.Identity?.Name, updatedCount);
                
                return Json(new { 
                    success = true, 
                    message = $"Updated {updatedCount} records with proper organism information",
                    updatedCount = updatedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unknown organisms");
                return Json(new { 
                    success = false, 
                    message = "Error updating organism information" 
                });
            }
        }

        /*! \fn Rescan
         * \brief HTTP POST for rescanning the assemblies
         * \return View of assemblies index
         */
        [HttpPost]
        public IActionResult Rescan()
        {
            _logger.LogInformation("Rescan action triggered by user: {User}", User.Identity?.Name);
            
            try
            {
                var blastDbPath = _configuration["Blast:BLASTDB"];
                _logger.LogInformation("BLAST database path configured as: {BlastDbPath}", blastDbPath);
                
                BackgroundJob.Enqueue<UpdateAssemblyDatabase>(x => x.Rescan(JobCancellationToken.Null));
                
                _logger.LogInformation("Background job for assembly database rescan has been enqueued");
                TempData["Alert"] = "A rescan has been triggered! It may take a few minutes.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while triggering assembly database rescan");
                TempData["Alert"] = $"Error occurred while triggering rescan: {ex.Message}. Please check the logs.";
            }

            return RedirectToAction(nameof(Index));
        }

        /*! \fn ToggleEnabled
         * \brief HTTP POST to toggle the enabled status of an assembly
         * \param id The taxonomy ID of the assembly to toggle
         * \return JSON result with success status
         */
        [HttpPost]
        public async Task<IActionResult> ToggleEnabled(int id)
        {
            _logger.LogInformation("Toggle enabled status for assembly with taxonomy ID: {TaxonomyId} by user: {User}", id, User.Identity?.Name);
            
            try
            {
                var assembly = await _context.Assemblies.FirstOrDefaultAsync(a => a.TaxonomyId == id);
                if (assembly == null)
                {
                    return Json(new { success = false, message = "Assembly not found" });
                }

                assembly.IsEnabled = !assembly.IsEnabled;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assembly {TaxonomyId} enabled status changed to: {IsEnabled}", id, assembly.IsEnabled);

                return Json(new { 
                    success = true, 
                    isEnabled = assembly.IsEnabled,
                    message = $"Assembly {(assembly.IsEnabled ? "enabled" : "disabled")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling assembly enabled status for ID: {TaxonomyId}", id);
                return Json(new { success = false, message = "An error occurred while updating the assembly status" });
            }
        }

        /*! \fn Delete
         * \brief HTTP POST to delete an assembly and its files
         * \param id The taxonomy ID of the assembly to delete
         * \return JSON result with success status
         */
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete assembly with taxonomy ID: {TaxonomyId} by user: {User}", id, User.Identity?.Name);
            
            try
            {
                var assembly = await _context.Assemblies.FirstOrDefaultAsync(a => a.TaxonomyId == id);
                if (assembly == null)
                {
                    return Json(new { success = false, message = "Assembly not found" });
                }

                // Check if assembly is being used by any active jobs
                var activeJobsCount = await _context.Jobs
                    .Where(j => j.AssemblyId == id && 
                               (j.JobState == JobState.New || j.JobState == JobState.Started ||
                                j.JobState == JobState.CandidateGenerator || j.JobState == JobState.Structure ||
                                j.JobState == JobState.MultiObjectiveOptimization || j.JobState == JobState.Specificity ||
                                j.JobState == JobState.QueuedPhase2 || j.JobState == JobState.QueuedPhase3))
                    .CountAsync();

                if (activeJobsCount > 0)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Cannot delete assembly. It is currently being used by {activeJobsCount} active job(s)." 
                    });
                }

                // Delete files from filesystem
                var deletedSize = 0L;
                if (!string.IsNullOrEmpty(assembly.Path) && Directory.Exists(assembly.Path))
                {
                    try
                    {
                        deletedSize = CalculateAssemblySize(assembly.Path);
                        Directory.Delete(assembly.Path, true);
                        _logger.LogInformation("Deleted assembly directory: {Path} ({Size} bytes)", assembly.Path, deletedSize);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete assembly directory: {Path}", assembly.Path);
                        return Json(new { 
                            success = false, 
                            message = $"Failed to delete assembly files from filesystem: {ex.Message}" 
                        });
                    }
                }

                // Remove from database
                _context.Assemblies.Remove(assembly);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assembly {TaxonomyId} ({OrganismName}) deleted successfully. Freed {Size} bytes.", 
                    id, assembly.OrganismName, deletedSize);

                return Json(new { 
                    success = true, 
                    message = $"Assembly '{assembly.OrganismName}' deleted successfully. Freed {FormatFileSize(deletedSize)}.",
                    deletedSize = deletedSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting assembly with ID: {TaxonomyId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the assembly" });
            }
        }

        /*! \fn CalculateAssemblySize
         * \brief Calculate the total size of an assembly directory
         * \param path Directory path to calculate
         * \return Total size in bytes
         */
        private long CalculateAssemblySize(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return 0;

            try
            {
                var directoryInfo = new DirectoryInfo(path);
                return directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate size for directory: {Path}", path);
                return 0;
            }
        }

        /*! \fn FormatFileSize
         * \brief Format file size in human-readable format
         * \param bytes Size in bytes
         * \return Formatted size string
         */
        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }

        /*! \fn ExpandPath
         * \brief Expand ~ and environment variables in path
         * \param path Path to expand
         * \return Expanded path
         */
        private static string ExpandPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Expand ~ to home directory
            if (path.StartsWith("~/") || path == "~")
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (path == "~")
                    return homeDir;
                return Path.Combine(homeDir, path.Substring(2));
            }

            // Expand environment variables
            return Environment.ExpandEnvironmentVariables(path);
        }

        /*! \fn DetermineAssemblyType
         * \brief Determine the actual assembly type based on available files
         * \param assemblyPath Path to the assembly directory
         * \param accessionId Accession ID for pattern matching
         * \return Assembly type string
         */
        private string DetermineAssemblyType(string assemblyPath, string accessionId)
        {
            if (string.IsNullOrEmpty(assemblyPath) || !Directory.Exists(assemblyPath))
            {
                return "Unknown";
            }

            try
            {
                var files = Directory.GetFiles(assemblyPath, "*", SearchOption.TopDirectoryOnly)
                    .Select(f => Path.GetFileName(f).ToLowerInvariant())
                    .ToList();

                // Count different types of BLAST databases
                var hasGenome = files.Any(f => f.Contains("genomic") && (f.EndsWith(".nhr") || f.EndsWith(".phr")));
                var hasProtein = files.Any(f => f.Contains("protein") && f.EndsWith(".phr"));
                var hasRNA = files.Any(f => f.Contains("rna") && f.EndsWith(".nhr"));
                var hasCDS = files.Any(f => f.Contains("cds") && f.EndsWith(".nhr"));

                // Determine primary type based on available databases
                var types = new List<string>();
                
                if (hasGenome) types.Add("Genome");
                if (hasProtein) types.Add("Proteome");
                if (hasRNA) types.Add("Transcriptome");
                if (hasCDS) types.Add("CDS");

                if (types.Count == 0)
                {
                    // Fallback: check for any BLAST database files
                    var hasAnyBlastDb = files.Any(f => f.EndsWith(".nhr") || f.EndsWith(".phr") || 
                                                      f.EndsWith(".nin") || f.EndsWith(".pin"));
                    return hasAnyBlastDb ? "Assembly" : "Unknown";
                }

                // Return combined type or primary type
                if (types.Count == 1)
                {
                    return types[0];
                }
                else if (types.Contains("Genome"))
                {
                    // If genome is present with others, it's likely a complete genome assembly
                    return "Complete Genome";
                }
                else
                {
                    // Multiple types without genome
                    return string.Join(" + ", types);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to determine assembly type for path: {Path}", assemblyPath);
                return "Unknown";
            }
        }
    }
}

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
         * \brief HTTP GET for viewing download history and status
         * \return View with download history
         */
        public async Task<IActionResult> Downloads()
        {
            try
            {
                var downloads = await _datasetDownloadService.GetDownloadHistoryAsync();
                return View(downloads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading download history");
                TempData["Error"] = "Error loading download history.";
                return RedirectToAction(nameof(Index));
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
    }
}

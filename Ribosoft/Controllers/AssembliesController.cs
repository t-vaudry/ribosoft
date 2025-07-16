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

        /*! \fn AssembliesController
         * \brief Default constructor
         * \param context Database context information
         * \param configuration Configuration of the application
         * \param logger Logger instance
         */
        public AssembliesController(ApplicationDbContext context, IConfiguration configuration, ILogger<AssembliesController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /*! \fn Index
         * \brief HTTP GET assemblies index
         * \return View of assemblies index
         */
        public async Task<IActionResult> Index()
        {
            ViewBag.BlastDbPath = _configuration["Blast:BLASTDB"] ?? "Not configured";
            return View(await _context.Assemblies.ToListAsync());
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

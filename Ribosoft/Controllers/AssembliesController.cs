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

        /*! \fn AssembliesController
         * \brief Default constructor
         * \param context Database context information
         * \param configuration Configuration of the application
         */
        public AssembliesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /*! \fn Index
         * \brief HTTP GET assemblies index
         * \return View of assemblies index
         */
        public async Task<IActionResult> Index()
        {
            return View(await _context.Assemblies.ToListAsync());
        }

        /*! \fn Rescan
         * \brief HTTP POST for rescanning the assemblies
         * \return View of assemblies index
         */
        public IActionResult Rescan()
        {
            BackgroundJob.Enqueue<UpdateAssemblyDatabase>(x => x.Rescan(JobCancellationToken.Null));    

            TempData["Alert"] = "A rescan has been triggered! It may take a few minutes.";

            return RedirectToAction(nameof(Index));
        }
    }
}

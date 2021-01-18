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
    [Authorize(Roles = "Administrator")]
    public class AssembliesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AssembliesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Assemblies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Assemblies.ToListAsync());
        }

        public IActionResult Rescan()
        {
            BackgroundJob.Enqueue<UpdateAssemblyDatabase>(x => x.Rescan(JobCancellationToken.Null));    

            TempData["Alert"] = "A rescan has been triggered! It may take a few minutes.";

            return RedirectToAction(nameof(Index));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Jobs;
using Ribosoft.Models;

namespace Ribosoft.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Jobs
        public async Task<IActionResult> Index()
        {
            var user = await GetUser();

            var applicationDbContext = _context.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Ribozyme)
                .Where(j => j.OwnerId == user.Id)
                .OrderByDescending(j => j.CreatedAt);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Jobs/Details/5
        public async Task<IActionResult> Details(int? id, string sortOrder)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await GetUser();

            var job = await _context.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Ribozyme)
                .Include(j => j.Designs)
                .Where(j => j.OwnerId == user.Id)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            var designs = from d in _context.Designs select d;
            switch (sortOrder)
            {
                case "temp_desc":
                    designs = designs.OrderByDescending(d => d.TemperatureScore);
                    break;
                case "spec_desc":
                    designs = designs.OrderByDescending(d => d.SpecializationScore);
                    break;
                case "access_desc":
                    designs = designs.OrderByDescending(d => d.AccessibilityScore);
                    break;
                case "struct_desc":
                    designs = designs.OrderByDescending(d => d.StructureScore);
                    break;
                default:
                    designs = designs.OrderBy(d => d.Rank);
                    break;
            }
            job.Designs = designs.ToList();

            return View(job);
        }

        // GET: Jobs/Create
        public IActionResult Create()
        {
            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name");
            return View();
        }

        // POST: Jobs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RibozymeId,RNAInput")] Job job)
        {
            var user = await GetUser();

            if (ModelState.IsValid)
            {
                _context.Add(job);
                job.OwnerId = user.Id;
                job.JobState = JobState.New;
                await _context.SaveChangesAsync();

                job.HangfireJobId = BackgroundJob.Enqueue<GenerateCandidates>(x => x.Generate(job.Id, JobCancellationToken.Null));
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name", job.RibozymeId);
            return View(job);
        }

        // GET: Jobs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await GetUser();

            var job = await _context.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Ribozyme)
                .Where(j => j.OwnerId == user.Id)
                .Where(j => j.JobState == JobState.New || j.JobState == JobState.Started)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await GetUser();

            var job = await _context.Jobs
                .Where(j => j.OwnerId == user.Id)
                .Where(j => j.JobState == JobState.New || j.JobState == JobState.Started)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            job.JobState = JobState.Cancelled;
            await _context.SaveChangesAsync();

            if (job.HangfireJobId != null)
            {
                BackgroundJob.Delete(job.HangfireJobId);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

        private async Task<ApplicationUser> GetUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return user;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using cloudscribe.Pagination.Models;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Jobs;
using Ribosoft.Models;
using Ribosoft.Models.JobsViewModels;

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
        public async Task<IActionResult> Index(int pageNumber)
        {
            var user = await GetUser();

            int pageSize = 20;
            pageNumber = Math.Max(pageNumber, 1);
            int offset = (pageSize * pageNumber) - pageSize;

            var vm = new JobIndexViewModel();

            var jobs = _context.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Ribozyme)
                .Where(j => j.OwnerId == user.Id)
                .OrderByDescending(j => j.CreatedAt);

            var inProgressJobs = jobs.Where(Job.InProgress());
            var completedJobs = jobs.Where(Job.Completed());

            vm.InProgress = inProgressJobs;
            vm.Completed.Data = await completedJobs.Skip(offset).Take(pageSize).AsNoTracking().ToListAsync();
            vm.Completed.TotalItems = await completedJobs.CountAsync();
            vm.Completed.PageNumber = pageNumber;
            vm.Completed.PageSize = pageSize;

            return View(vm);
        }

        // GET: Jobs/Details/5
        public async Task<IActionResult> Details(int? id, string sortOrder, int pageNumber, string filterParam, string filterCondition, float filterValue)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await GetUser();

            var job = await _context.Jobs
                .Include(j => j.Owner)
                .Include(j => j.Ribozyme)
                .Include(j => j.Assembly)
                .Where(j => j.OwnerId == user.Id)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            var designs = from d in _context.Designs where d.JobId == job.Id select d;

            if (!String.IsNullOrEmpty(filterParam))
            {
                StringBuilder sb = new StringBuilder(filterValue.ToString());
                sb[sb.Length-1] = Convert.ToChar(Convert.ToInt32(sb[sb.Length-1])+1);
                float upperBound = float.Parse(sb.ToString());

                switch (filterParam)
                {
                    case "Rank":
                        if (filterCondition == "gteq") {
                            designs = designs.Where(d => d.Rank >= filterValue);
                        } else if (filterCondition == "lteq") {
                            designs = designs.Where(d => d.Rank <= filterValue);
                        } else if (filterCondition == "eq") {
                            designs = designs.Where(d => d.Rank == (int) filterValue);
                        }
                        break;
                    case "HighestTemperatureScore":
                        if (filterCondition == "gteq") {
                            designs = designs.Where(d => d.HighestTemperatureScore >= filterValue);
                        } else if (filterCondition == "lteq") {
                            designs = designs.Where(d => d.HighestTemperatureScore <= filterValue);
                        } else if (filterCondition == "eq") {
                            designs = designs.Where(d => d.HighestTemperatureScore >= filterValue && d.HighestTemperatureScore < upperBound);
                        }
                        break;
                    case "DesiredTemperatureScore":
                        if (filterCondition == "gteq") {
                            designs = designs.Where(d => d.DesiredTemperatureScore >= filterValue);
                        } else if (filterCondition == "lteq") {
                            designs = designs.Where(d => d.DesiredTemperatureScore <= filterValue);
                        } else if (filterCondition == "eq") {
                            designs = designs.Where(d => d.DesiredTemperatureScore >= filterValue && d.DesiredTemperatureScore < upperBound);
                        }
                        break;
                    case "SpecificityScore":
                        if (filterCondition == "gteq") {
                            designs = designs.Where(d => d.SpecificityScore >= filterValue);
                        } else if (filterCondition == "lteq") {
                            designs = designs.Where(d => d.SpecificityScore <= filterValue);
                        } else if (filterCondition == "eq") {
                            designs = designs.Where(d => d.SpecificityScore >= filterValue && d.SpecificityScore < upperBound);
                        }
                        break;
                    case "AcessibilityScore":
                        if (filterCondition == "gteq") {
                            designs = designs.Where(d => d.AccessibilityScore >= filterValue);
                        } else if (filterCondition == "lteq") {
                            designs = designs.Where(d => d.AccessibilityScore <= filterValue);
                        } else if (filterCondition == "eq") {
                            designs = designs.Where(d => d.AccessibilityScore >= filterValue && d.AccessibilityScore < upperBound);
                        }
                        break;
                    case "StructureScore":
                        if (filterCondition == "gteq") {
                            designs = designs.Where(d => d.StructureScore >= filterValue);
                        } else if (filterCondition == "lteq") {
                            designs = designs.Where(d => d.StructureScore <= filterValue);
                        } else if (filterCondition == "eq") {
                            designs = designs.Where(d => d.StructureScore >= filterValue && d.StructureScore < upperBound);
                        }
                        break;
                    default:
                        break;
                }
            }

            int pageSize = 20;
            pageNumber = Math.Max(pageNumber, 1);
            int offset = (pageSize * pageNumber) - pageSize;

            switch (sortOrder)
            {
                case "desired_temp_desc":
                    designs = designs.OrderByDescending(d => d.DesiredTemperatureScore);
                    break;
                case "high_temp_desc":
                    designs = designs.OrderByDescending(d => d.HighestTemperatureScore);
                    break;
                case "spec_desc":
                    designs = designs.OrderByDescending(d => d.SpecificityScore);
                    break;
                case "access_desc":
                    designs = designs.OrderByDescending(d => d.AccessibilityScore);
                    break;
                case "struct_desc":
                    designs = designs.OrderByDescending(d => d.StructureScore);
                    break;
                case "rank_asc":
                default:
                    designs = designs.OrderBy(d => d.Rank);
                    break;
            }

            List<SelectListItem> filterParams = new List<SelectListItem>();
            List<SelectListItem> filterConditions = new List<SelectListItem>();

            filterParams.Add(new SelectListItem { Text = "Rank", Value = "Rank", Selected=true });
            filterParams.Add(new SelectListItem { Text = "Highest Temperature Score", Value = "HighestTemperatureScore" });
            filterParams.Add(new SelectListItem { Text = "Desired Temperature Score", Value = "DesiredTemperatureScore" });
            filterParams.Add(new SelectListItem { Text = "Specificity Score", Value = "SpecificityScore" });
            filterParams.Add(new SelectListItem { Text = "Acessibility Score", Value = "AcessibilityScore" });
            filterParams.Add(new SelectListItem { Text = "Structure Score", Value = "StructureScore" });

            filterConditions.Add(new SelectListItem { Text = ">=", Value = "gteq", Selected=true });
            filterConditions.Add(new SelectListItem { Text = "<=", Value = "lteq" });
            filterConditions.Add(new SelectListItem { Text = "=", Value = "eq" });

            var vm = new JobDetailsViewModel();

            vm.Job = job;
            vm.SortOrder = sortOrder;
            vm.FilterParams = filterParams;
            vm.FilterConditions = filterConditions;
            vm.FilterParam = filterParam;
            vm.FilterCondition = filterCondition;
            vm.FilterValue = filterValue.ToString();
            vm.Designs.Data = await designs.Skip(offset).Take(pageSize).AsNoTracking().ToListAsync();
            vm.Designs.TotalItems = await designs.CountAsync();
            vm.Designs.PageNumber = pageNumber;
            vm.Designs.PageSize = pageSize;

            return View(vm);
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

                job.HangfireJobId = BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase1(job.Id, JobCancellationToken.Null));
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
                .SingleOrDefaultAsync(m => m.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            if (job.IsInProgress())
            {
                job.JobState = JobState.Cancelled;

                if (job.HangfireJobId != null)
                {
                    BackgroundJob.Delete(job.HangfireJobId);
                    job.HangfireJobId = null;
                }
            }
            else
            {
                _context.Jobs.Remove(job);
            }

            await _context.SaveChangesAsync();

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

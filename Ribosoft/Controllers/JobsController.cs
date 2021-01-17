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

using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Newtonsoft.Json.Linq;
using System.Web.Helpers;

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
        public async Task<IActionResult> Index(int pageNumber, string eMessage = "", string sMessage = "")
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
            vm.ErrorMessage = eMessage;
            vm.SuccessMessage = sMessage;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(JobIndexViewModel model)
        {
            int val;
            if (Int32.TryParse(model.JobId, out val) && _context.Jobs.Any(job => job.Id == val))
            {
                // modify owner of job to current user
                ApplicationUser user = await GetUser();
                Job job = await _context.Jobs.SingleOrDefaultAsync(m => m.Id == Int32.Parse(model.JobId));
                job.OwnerId = user.Id;
                _context.Jobs.Update(job);
                await _context.SaveChangesAsync();
                model.SuccessMessage = "Successfully added Job!";
            }
            else
            {
                model.ErrorMessage = "Invalid Job Id!";
            }

            return RedirectToAction(nameof(Index), new { pageNumber = 1, eMessage = model.ErrorMessage, sMessage = model.SuccessMessage } );
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
                FilterDesigns(ref designs, filterParam, filterCondition, filterValue);
            }

            int pageSize = 20;
            pageNumber = Math.Max(pageNumber, 1);
            int offset = (pageSize * pageNumber) - pageSize;

            ViewBag.DesTempColumnTitle = "Desired Temperature Score";
            ViewBag.HiTempColumnTitle = "Highest Temperature Score";
            ViewBag.SpecColumnTitle = "Specificity Score";
            ViewBag.AccessColumnTitle = "Accessibility Score";
            ViewBag.StructColumnTitle = "Structure Score";
            ViewBag.RankColumnTitle = "Rank";

            ViewBag.DesTempSortParm = sortOrder == "des_temp_asc" ? "des_temp_desc" : "des_temp_asc";
            ViewBag.HiTempSortParm = sortOrder == "hi_temp_asc" ? "hi_temp_desc" : "hi_temp_asc";
            ViewBag.SpecSortParm = sortOrder == "spec_asc" ? "spec_desc" : "spec_asc";
            ViewBag.AccessSortParm = sortOrder == "access_asc" ? "access_desc" : "access_asc";
            ViewBag.StructSortParm = sortOrder == "struct_asc" ? "struct_desc" : "struct_asc";
            ViewBag.RankSortParm = sortOrder == "rank_asc" ? "rank_desc" : "rank_asc";
            // ViewBag.RankSortParm = String.IsNullOrEmpty(sortOrder) ? "rank_asc" : "";

            switch (sortOrder)
            {
                case "des_temp_desc":
                    designs = designs.OrderByDescending(d => d.DesiredTemperatureScore);
                    ViewBag.DesTempColumnTitle = "▼ |  " + ViewBag.DesTempColumnTitle;
                    break;
                case "des_temp_asc":
                    designs = designs.OrderBy(d => d.DesiredTemperatureScore);
                    ViewBag.DesTempColumnTitle = "▲ | " + ViewBag.DesTempColumnTitle;
                    break;
                case "hi_temp_desc":
                    designs = designs.OrderByDescending(d => d.HighestTemperatureScore);
                    ViewBag.HiTempColumnTitle = "▼ | " + ViewBag.HiTempColumnTitle;
                    break;
                case "hi_temp_asc":
                    designs = designs.OrderBy(d => d.HighestTemperatureScore);
                    ViewBag.HiTempColumnTitle = "▲ | " + ViewBag.HiTempColumnTitle;
                    break;
                case "spec_desc":
                    designs = designs.OrderByDescending(d => d.SpecificityScore);
                    ViewBag.SpecColumnTitle = "▼ " + ViewBag.SpecColumnTitle;
                    break;
                case "spec_asc":
                    designs = designs.OrderBy(d => d.SpecificityScore);
                    ViewBag.SpecColumnTitle = "▲ | " + ViewBag.SpecColumnTitle;
                    break;
                case "access_desc":
                    designs = designs.OrderByDescending(d => d.AccessibilityScore);
                    ViewBag.AccessColumnTitle = "▼ | " + ViewBag.AccessColumnTitle;
                    break;
                case "access_asc":
                    designs = designs.OrderBy(d => d.AccessibilityScore);
                    ViewBag.AccessColumnTitle = "▲ | " + ViewBag.AccessColumnTitle;
                    break;
                case "struct_desc":
                    designs = designs.OrderByDescending(d => d.StructureScore);
                    ViewBag.StructColumnTitle = "▼ | " + ViewBag.StructColumnTitle;
                    break;
                case "struct_asc":
                    designs = designs.OrderBy(d => d.StructureScore);
                    ViewBag.StructColumnTitle = "▲ | " + ViewBag.StructColumnTitle;
                    break;
                case "rank_desc":
                    designs = designs.OrderByDescending(d => d.Rank);
                    ViewBag.RankColumnTitle = "▼ | " + ViewBag.RankColumnTitle;
                    break;
                default:
                    designs = designs.OrderBy(d => d.Rank);
                    ViewBag.RankColumnTitle = "▲ | " + ViewBag.RankColumnTitle;
                    break;
            }

            List<SelectListItem> filterParams = new List<SelectListItem>();
            List<SelectListItem> filterConditions = new List<SelectListItem>();

            filterParams.Add(new SelectListItem { Text = "Rank", Value = "Rank", Selected = true });
            filterParams.Add(new SelectListItem { Text = "Highest Temperature Score", Value = "HighestTemperatureScore" });
            filterParams.Add(new SelectListItem { Text = "Desired Temperature Score", Value = "DesiredTemperatureScore" });
            filterParams.Add(new SelectListItem { Text = "Specificity Score", Value = "SpecificityScore" });
            filterParams.Add(new SelectListItem { Text = "Acessibility Score", Value = "AcessibilityScore" });
            filterParams.Add(new SelectListItem { Text = "Structure Score", Value = "StructureScore" });

            filterConditions.Add(new SelectListItem { Text = ">=", Value = "gteq", Selected = true });
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

        public FileStreamResult DownloadDesigns(int jobID, string format, string filterParam, string filterCondition, float filterValue)
        {
            var designs = from d in _context.Designs where d.JobId == jobID select d;
            if (!String.IsNullOrEmpty(filterParam))
            {
                FilterDesigns(ref designs, filterParam, filterCondition, filterValue);                
            }
            var payload = "";
            var extension = "";
            var type = "";
            MemoryStream stream = new MemoryStream();
            byte[] byteArray;
            switch (format)
            {
                case "csv":
                    payload += String.Format("Rank,DesiredTemperatureScore,HighestTemperatureScore,SpecificityScore,AccessibilityScore,StructureScore,CreatedAt,UpdatedAt,Sequence\n");
                    extension = "csv";
                    foreach (Design d in designs)
                    {
                        payload += String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n", d.Rank, d.DesiredTemperatureScore, d.HighestTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence);
                    }
                    type = "text/plain";
                    byteArray = Encoding.ASCII.GetBytes(payload);
                    stream = new MemoryStream(byteArray);
                    break;
                case "zip":
                    extension = "zip";
                    type = "application/octet-stream";
                    ZipOutputStream zipStream = new ZipOutputStream(stream);
                    foreach (Design d in designs)
                    {
                        var newEntry = new ZipEntry("design" + d.Id + ".fasta");
                        newEntry.DateTime = DateTime.Now;

                        zipStream.PutNextEntry(newEntry);
                        byteArray = Encoding.ASCII.GetBytes(String.Format(">Rank {0} | DesiredTemperatureScore {1} | HighestTemperatureScore {2} | SpecificityScore {3} | AccessibilityScore {4} | StructureScore {5} | CreatedAt {6} | UpdatedAt {7}\n{8}\n\n", d.Rank, d.DesiredTemperatureScore, d.HighestTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence));

                        MemoryStream inStream = new MemoryStream(byteArray);
                        StreamUtils.Copy(inStream, zipStream, new byte[4096]);
                        inStream.Close();
                        zipStream.CloseEntry();
                    }

                    zipStream.IsStreamOwner = false;
                    zipStream.Close();
                    stream.Position = 0;
                    break;
                case "fasta":
                    extension = "fasta";
                    foreach (Design d in designs)
                    {
                        payload += String.Format(">Rank {0} | DesiredTemperatureScore {1} | HighestTemperatureScore {2} | SpecificityScore {3} | AccessibilityScore {4} | StructureScore {5} | CreatedAt {6} | UpdatedAt {7}\n{8}\n\n", d.Rank, d.DesiredTemperatureScore, d.HighestTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence);
                    }
                    type = "text/plain";
                    byteArray = Encoding.ASCII.GetBytes(payload);
                    stream = new MemoryStream(byteArray);
                    break;
            }

            return File(stream, type, String.Format("job{0}_bulk.{1}", jobID, extension));
        }

        public JsonResult DownloadSelectedDesigns(int jobID, string selectedDesigns, string format)
        {
            JObject obj = JObject.Parse(selectedDesigns);
            string handle = Guid.NewGuid().ToString();
            var designs = from d in _context.Designs where d.JobId == jobID select d;
            var payload = "";
            var extension = "";
            var type = "";
            MemoryStream stream = new MemoryStream();
            byte[] byteArray;
            switch (format)
            {
                case "csv":
                    payload += String.Format("Rank,DesiredTemperatureScore,HighestTemperatureScore,SpecificityScore,AccessibilityScore,StructureScore,CreatedAt,UpdatedAt,Sequence\n");
                    extension = "csv";
                    foreach (Design d in designs)
                    {
                        if (obj.ContainsKey(jobID.ToString() + '-' + d.Id))
                        {
                            payload += String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n", d.Rank, d.DesiredTemperatureScore, d.HighestTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence);
                        }
                    }
                    type = "application/csv";
                    byteArray = Encoding.ASCII.GetBytes(payload);
                    stream = new MemoryStream(byteArray);
                    break;
                case "zip":
                    extension = "zip";
                    type = "application/octet-stream";
                    ZipOutputStream zipStream = new ZipOutputStream(stream);
                    foreach (Design d in designs)
                    {
                        if (obj.ContainsKey(jobID.ToString() + '-' + d.Id))
                        {
                            var newEntry = new ZipEntry("design" + d.Id + ".fasta");
                            newEntry.DateTime = DateTime.Now;

                            zipStream.PutNextEntry(newEntry);
                            byteArray = Encoding.ASCII.GetBytes(String.Format(">Rank {0} | DesiredTemperatureScore {1} | HighestTemperatureScore {2} | SpecificityScore {3} | AccessibilityScore {4} | StructureScore {5} | CreatedAt {6} | UpdatedAt {7}\n{8}\n\n", d.Rank, d.DesiredTemperatureScore, d.HighestTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence));

                            MemoryStream inStream = new MemoryStream(byteArray);
                            StreamUtils.Copy(inStream, zipStream, new byte[4096]);
                            inStream.Close();
                            zipStream.CloseEntry();
                        }
                    }

                    zipStream.IsStreamOwner = false;
                    zipStream.Close();
                    stream.Position = 0;
                    break;
                case "fasta":
                    extension = "fasta";
                    foreach (Design d in designs)
                    {
                        if (obj.ContainsKey(jobID.ToString() + '-' + d.Id))
                        {
                            payload += String.Format(">Rank {0} | DesiredTemperatureScore {1} | HighestTemperatureScore {2} | SpecificityScore {3} | AccessibilityScore {4} | StructureScore {5} | CreatedAt {6} | UpdatedAt {7}\n{8}\n\n", d.Rank, d.DesiredTemperatureScore, d.HighestTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence);
                        }
                    }
                    type = "text/plain";
                    byteArray = Encoding.ASCII.GetBytes(payload);
                    stream = new MemoryStream(byteArray);
                    break;
            }

            TempData[handle] = Convert.ToBase64String(stream.ToArray());
            return new JsonResult(new { FileGuid = handle, FileName = String.Format("job{0}_bulk.{1}", jobID, extension), FileType = type });
        }

        [HttpGet]
        public virtual ActionResult Download(string fileGuid, string fileName, string fileType)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = Convert.FromBase64String(TempData[fileGuid] as string);
                return File(data, fileType, fileName);
            }
            else
            {
                return new EmptyResult();
            }
        }

        private void FilterDesigns(ref IQueryable<Design> designs, string filterParam, string filterCondition, float filterValue)
        {
            StringBuilder sb = new StringBuilder(filterValue.ToString());
            sb[sb.Length - 1] = Convert.ToChar(Convert.ToInt32(sb[sb.Length - 1]) + 1);
            float upperBound = float.Parse(sb.ToString());
            switch(filterCondition)
            {
                case "gteq":
                    GreaterThanOrEqualTo(ref designs, filterParam, filterValue);
                    break;
                case "lteq":
                    LessThanOrEqualTo(ref designs, filterParam, filterValue);
                    break;
                case "eq":
                    EqualTo(ref designs, filterParam, filterValue, upperBound);
                    break;
                default:
                    break;
            }
        }

        private void GreaterThanOrEqualTo(ref IQueryable<Design> designs, string filterParam, float filterValue)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank >= filterValue);
                    break;
                case "HighestTemperatureScore":
                    designs = designs.Where(d => d.HighestTemperatureScore >= filterValue);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore >= filterValue);
                    break;
                case "SpecificityScore":
                    designs = designs.Where(d => d.SpecificityScore >= filterValue);
                    break;
                case "AcessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore >= filterValue);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore >= filterValue);
                    break;
                default:
                    break;
            }
        }

        private void LessThanOrEqualTo(ref IQueryable<Design> designs, string filterParam, float filterValue)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank <= filterValue);
                    break;
                case "HighestTemperatureScore":
                    designs = designs.Where(d => d.HighestTemperatureScore <= filterValue);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore <= filterValue);
                    break;
                case "SpecificityScore":
                    designs = designs.Where(d => d.SpecificityScore <= filterValue);
                    break;
                case "AcessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore <= filterValue);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore <= filterValue);
                    break;
                default:
                    break;
            }
        }

        private void EqualTo(ref IQueryable<Design> designs, string filterParam, float filterValue, float upperBound)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank == (int)filterValue);
                    break;
                case "HighestTemperatureScore":
                    designs = designs.Where(d => d.HighestTemperatureScore >= filterValue && d.HighestTemperatureScore < upperBound);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore >= filterValue && d.DesiredTemperatureScore < upperBound);
                    break;
                case "SpecificityScore":
                     designs = designs.Where(d => d.SpecificityScore >= filterValue && d.SpecificityScore < upperBound);
                    break;
                case "AcessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore >= filterValue && d.AccessibilityScore < upperBound);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore >= filterValue && d.StructureScore < upperBound);
                    break;
                default:
                    break;
            }
        }
    }
}

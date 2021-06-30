using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using cloudscribe.Pagination.Models;
using Hangfire;
using Microsoft.AspNetCore.Http;
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
using Newtonsoft.Json;
using static Ribosoft.Models.JobsViewModels.JobDetailsViewModel;

namespace Ribosoft.Controllers
{
    /*! \class JobsController
     * \brief Controller class for the jobs
     */
    public class JobsController : Controller
    {
        /*! \property _context
         * \brief Local application database context
         */
        private readonly ApplicationDbContext _context;

        /*! \property _userManager
         * \brief Local user manager
         */
        private readonly UserManager<ApplicationUser> _userManager;

        /*! \fn JobsController
         * \brief Default constructor
         * \param context Application database context
         * \param userManager User manager
         */
        public JobsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /*!
         * \brief HTTP GET request for job main page
         * \param model Model of the job index view
         * \return View of the job index
         */
        public async Task<IActionResult> Index(JobIndexViewModel model)
        {
            var user = await GetUser();
            return View(await GetViewModel(user, model));
        }

        /*! \fn AddJob
         * \brief HTTP POST request for job form submission
         * This request will attempt to transfer ownership of a job to the current user
         * \param model Model of job index view
         * \return View of the job index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddJob(JobIndexViewModel model)
        {
            ApplicationUser user = await GetUser();
            model.ErrorMessages = new List<string>();
            model.SuccessMessages = new List<string>();

            int val;
            if (Int32.TryParse(model.JobId, out val) && _context.Jobs.Any(job => job.Id == val))
            {
                // modify owner of job to current user
                Job job = await _context.Jobs.SingleOrDefaultAsync(m => m.Id == Int32.Parse(model.JobId));
                job.OwnerId = user.Id;
                _context.Jobs.Update(job);
                await _context.SaveChangesAsync();
                model.SuccessMessages.Add(String.Format("Successfully added Job [{0}]!", model.JobId));
            }
            else
            {
                model.ErrorMessages.Add(String.Format("Invalid Job Id [{0}]!", model.JobId));
            }

            return View("Index", await GetViewModel(user, model));
        }

        /*! \fn DownloadJobs
         * \brief HTTP POST request to download CSV of job ids
         * This request will generate a CSV of all the current user's job ids
         * \return File for download
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<FileStreamResult> DownloadJobs()
        {
            var user = await GetUser();
            var jobs = from j in _context.Jobs where j.OwnerId == user.Id select j;
            var payload = "";

            foreach (Job j in jobs)
            {
                payload += j.Id.ToString() + ',';
            }
            payload = payload.Remove(payload.Length - 1);

            byte[] byteArray = Encoding.ASCII.GetBytes(payload);
            MemoryStream stream = new MemoryStream(byteArray);
            return File(stream, "application/csv", "jobs.csv");
        }

        /*! \fn UploadJobs
         * \brief HTTP POST request to bulk upload jobs
         * This request will attempt to transfer ownership of jobs to the current user
         * \param model Model of job index view
         * \return View of the job index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadJobs(JobIndexViewModel model)
        {
            ApplicationUser user = await GetUser();
            model.ErrorMessages = new List<string>();
            model.SuccessMessages = new List<string>();

            if (model.UploadFile.Length > 0)
            {
                var result = new StringBuilder();
                using (var reader = new StreamReader(model.UploadFile.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        result.AppendLine(reader.ReadLine());
                }
                string data = result.ToString();
                foreach (var id in data.Split(',').Select(sValue => sValue.Trim()).ToArray())
                {
                    int val;
                    if (Int32.TryParse(id, out val) && _context.Jobs.Any(job => job.Id == val))
                    {
                        // modify owner of job to current user
                        Job job = await _context.Jobs.SingleOrDefaultAsync(m => m.Id == Int32.Parse(id));
                        job.OwnerId = user.Id;
                        _context.Jobs.Update(job);
                        await _context.SaveChangesAsync();
                        model.SuccessMessages.Add(String.Format("Successfully added Job [{0}]!", id));
                    }
                    else
                    {
                        model.ErrorMessages.Add(String.Format("Invalid Job Id [{0}]!", id));
                    }
                }
            }

            return View("Index", await GetViewModel(user, model));
        }

        /*!
         * \brief HTTP GET request for job details page
         * \param id Job ID
         * \param sortOrder String of current sort order
         * \param pageNumber Page number of the details
         * \param filterData Base64 encoded string of JSON filter data
         * \return View of the details index
         */
        public async Task<IActionResult> Details(int? id, string sortOrder, int pageNumber, string filterData)
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

            List<Filter> filterList = new List<Filter>();
            if (!string.IsNullOrEmpty(filterData))
            {
                filterData = Encoding.UTF8.GetString(Convert.FromBase64String(filterData));
                filterList = JsonConvert.DeserializeObject<List<Filter>>(filterData);
                foreach (var filter in filterList)
                {
                    FilterDesigns(ref designs, filter.param, filter.condition, float.Parse(filter.value));
                }
            }

            int pageSize = 20;
            pageNumber = Math.Max(pageNumber, 1);
            int offset = (pageSize * pageNumber) - pageSize;

            SetSortParams(sortOrder);
            SortDesigns(ref designs, sortOrder);

            var vm = new JobDetailsViewModel();

            vm.Job = job;
            vm.SortOrder = sortOrder;
            vm.FilterList = filterList;
            vm.Designs.Data = await designs.Skip(offset).Take(pageSize).AsNoTracking().ToListAsync();
            vm.Designs.TotalItems = await designs.CountAsync();
            vm.Designs.PageNumber = pageNumber;
            vm.Designs.PageSize = pageSize;

            return View(vm);
        }

        /*!
         * \brief HTTP GET request for job create page
         * \return View of the create index
         */
        public IActionResult Create()
        {
            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name");
            return View();
        }

        /*!
         * \brief HTTP POST request for job create form submission
         * \param job Job to create
         * \return View of the details index
         */
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

        /*! \fn Delete
         * \brief HTTP GET request for delete job page
         * \param id Job ID
         * \return View of the job delete index
         */
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

        /*! \fn DeleteConfirmed
         * \brief HTTP POST request to confirm deletion of job
         * \param id Job ID
         * \return View of the jobs index
         */
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

        /*! \fn JobExists
         * \brief Helper function to check if job exists
         * \param id Job ID
         * \return Boolean results from the check
         */
        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }

        /*! \fn GetUser
         * \brief Helper function to retrieve the current user
         * \return Current application user
         */
        private async Task<ApplicationUser> GetUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return user;
        }

        /*! \fn DownloadDesigns
         * \brief HTTP POST request to bulk download designs
         * \param jobID Job ID
         * \param filterData Base64 encoded string of JSON filter data
         * \param format File format (ie. CSV, FASTA, ZIP)
         * \return JSON results of files to download
         */
        public JsonResult DownloadDesigns(int jobID, string filterData, string format)
        {
            var designs = from d in _context.Designs where d.JobId == jobID select d;
            if (!string.IsNullOrEmpty(filterData))
            {
                filterData = Encoding.UTF8.GetString(Convert.FromBase64String(filterData));
                var filterList = JsonConvert.DeserializeObject<List<Filter>>(filterData);
                foreach (var filter in filterList)
                {
                    FilterDesigns(ref designs, filter.param, filter.condition, float.Parse(filter.value));
                }
            }
            string handle = Guid.NewGuid().ToString();
            MemoryStream stream;
            string extension, type;
            DownloadFiles(designs, null, format, out stream, out extension, out type);
            TempData[handle] = Convert.ToBase64String(stream.ToArray());
            return new JsonResult(new { FileGuid = handle, FileName = String.Format("job{0}_bulk.{1}", jobID, extension), FileType = type });
        }

        /*! \fn DownloadSelectedDesigns
         * \brief HTTP POST request to bulk download selected designs
         * \param jobID Job ID
         * \param selectedDesigns List of selected design ids
         * \param format File format (ie. CSV, FASTA, ZIP)
         * \return JSON results of files to download
         */
        public JsonResult DownloadSelectedDesigns(int jobID, string selectedDesigns, string format)
        {
            JObject obj = JObject.Parse(selectedDesigns);
            string handle = Guid.NewGuid().ToString();
            var designs = from d in _context.Designs where d.JobId == jobID select d;
            MemoryStream stream;
            string extension, type;
            DownloadFiles(designs, obj, format, out stream, out extension, out type );
            TempData[handle] = Convert.ToBase64String(stream.ToArray());
            return new JsonResult(new { FileGuid = handle, FileName = String.Format("job{0}_bulk.{1}", jobID, extension), FileType = type });
        }

        /*! \fn Download
         * \brief HTTP GET request to download files stored in temp storage
         * \param fileGuid Guid referencing temp storage elements
         * \param fileName Name of file to download
         * \param fileType File type
         * \return File stream of downloaded designs
         */
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

        /*! \fn GetViewModel
         * \brief Helper function to get the view model object
         * \param user Current user
         * \param model Current model
         * \return View model object
         */
        private async Task<JobIndexViewModel> GetViewModel(ApplicationUser user, JobIndexViewModel model)
        {
            int pageSize = 20;
            model.Completed.PageNumber = Math.Max(model.Completed.PageNumber, 1);
            int offset = (int)((pageSize * model.Completed.PageNumber) - pageSize);

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
            vm.Completed.PageNumber = model.Completed.PageNumber;
            vm.Completed.PageSize = pageSize;
            vm.ErrorMessages = model.ErrorMessages;
            vm.SuccessMessages = model.SuccessMessages;

            return vm;
        }

        /*! \fn SetSortParams
         * \brief Helper function to set current sort parameters
         * \param sortOrder String containing sorting information
         */
        private void SetSortParams(string sortOrder)
        {
            ViewBag.DesTempSortParm = sortOrder == "destemp_asc" ? "destemp_desc" : "destemp_asc";
            ViewBag.SpecSortParm = sortOrder == "spec_asc" ? "spec_desc" : "spec_asc";
            ViewBag.AccessSortParm = sortOrder == "access_asc" ? "access_desc" : "access_asc";
            ViewBag.StructSortParm = sortOrder == "struct_asc" ? "struct_desc" : "struct_asc";
            ViewBag.RankSortParm = sortOrder == "rank_asc" ? "rank_desc" : "rank_asc";
        }

        /*! \fn SortDesigns
         * \brief Helper function to sort designs
         * \param designs List of designs
         * \param sortOrder String containing sorting information
         */
        private void SortDesigns(ref IQueryable<Design> designs, string sortOrder)
        {
            switch (sortOrder)
            {
                case "destemp_desc":
                    designs = designs.OrderByDescending(d => d.DesiredTemperatureScore);
                    break;
                case "destemp_asc":
                    designs = designs.OrderBy(d => d.DesiredTemperatureScore);
                    break;
                case "spec_desc":
                    designs = designs.OrderByDescending(d => d.SpecificityScore);
                    break;
                case "spec_asc":
                    designs = designs.OrderBy(d => d.SpecificityScore);
                    break;
                case "access_desc":
                    designs = designs.OrderByDescending(d => d.AccessibilityScore);
                    break;
                case "access_asc":
                    designs = designs.OrderBy(d => d.AccessibilityScore);
                    break;
                case "struct_desc":
                    designs = designs.OrderByDescending(d => d.StructureScore);
                    break;
                case "struct_asc":
                    designs = designs.OrderBy(d => d.StructureScore);
                    break;
                case "rank_desc":
                    designs = designs.OrderByDescending(d => d.Rank);
                    break;
                default:
                    designs = designs.OrderBy(d => d.Rank);
                    break;
            }
        }

        /*! \fn FilterDesigns
         * \brief Helper function to filter designs
         * \param designs List of designs
         * \param filterParam String value of the parameter to filter on
         * \param filterCondition String value of which condition to apply to filter
         * \param filterValue Value of the value to filter
         */
        private void FilterDesigns(ref IQueryable<Design> designs, string filterParam, string filterCondition, float filterValue)
        {
            StringBuilder sb = new StringBuilder(filterValue.ToString());
            sb[sb.Length - 1] = Convert.ToChar(Convert.ToInt32(sb[sb.Length - 1]) + 1);
            float upperBound = float.Parse(sb.ToString());
            switch(filterCondition)
            {
                case "gt":
                    GreaterThan(ref designs, filterParam, filterValue);
                    break;
                case "lt":
                    LessThan(ref designs, filterParam, filterValue);
                    break;
                case "eq":
                    EqualTo(ref designs, filterParam, filterValue, upperBound);
                    break;
                case "ne":
                    NotEqualTo(ref designs, filterParam, filterValue, upperBound);
                    break;
                default:
                    break;
            }
        }

        /*! \fn GreaterThan
         * \brief Helper function to filter by greater than
         * \param designs List of designs
         * \param filterParam String value of the parameter to filter on
         * \param filterValue Value of the value to filter
         */
        private void GreaterThan(ref IQueryable<Design> designs, string filterParam, float filterValue)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank > filterValue);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore > filterValue);
                    break;
                case "SpecificityScore":
                    designs = designs.Where(d => d.SpecificityScore > filterValue);
                    break;
                case "AccessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore > filterValue);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore > filterValue);
                    break;
                default:
                    break;
            }
        }

        /*! \fn LessThan
         * \brief Helper function to filter by less than
         * \param designs List of designs
         * \param filterParam String value of the parameter to filter on
         * \param filterValue Value of the value to filter
         */
        private void LessThan(ref IQueryable<Design> designs, string filterParam, float filterValue)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank < filterValue);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore < filterValue);
                    break;
                case "SpecificityScore":
                    designs = designs.Where(d => d.SpecificityScore < filterValue);
                    break;
                case "AccessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore < filterValue);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore < filterValue);
                    break;
                default:
                    break;
            }
        }

        /*! \fn EqualTo
         * \brief Helper function to filter by equal to
         * \param designs List of designs
         * \param filterParam String value of the parameter to filter on
         * \param filterValue Value of the value to filter
         * \param upperBound Upper bound value for equality
         */
        private void EqualTo(ref IQueryable<Design> designs, string filterParam, float filterValue, float upperBound)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank == (int)filterValue);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore >= filterValue && d.DesiredTemperatureScore < upperBound);
                    break;
                case "SpecificityScore":
                     designs = designs.Where(d => d.SpecificityScore >= filterValue && d.SpecificityScore < upperBound);
                    break;
                case "AccessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore >= filterValue && d.AccessibilityScore < upperBound);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore >= filterValue && d.StructureScore < upperBound);
                    break;
                default:
                    break;
            }
        }

        /*! \fn NotEqualTo
         * \brief Helper function to filter by not equal to
         * \param designs List of designs
         * \param filterParam String value of the parameter to filter on
         * \param filterValue Value of the value to filter
         * \param upperBound Upper bound value for equality
         */
        private void NotEqualTo(ref IQueryable<Design> designs, string filterParam, float filterValue, float upperBound)
        {
            switch (filterParam)
            {
                case "Rank":
                    designs = designs.Where(d => d.Rank != (int)filterValue);
                    break;
                case "DesiredTemperatureScore":
                    designs = designs.Where(d => d.DesiredTemperatureScore < filterValue || d.DesiredTemperatureScore > upperBound);
                    break;
                case "SpecificityScore":
                    designs = designs.Where(d => d.SpecificityScore < filterValue || d.SpecificityScore > upperBound);
                    break;
                case "AccessibilityScore":
                    designs = designs.Where(d => d.AccessibilityScore < filterValue || d.AccessibilityScore > upperBound);
                    break;
                case "StructureScore":
                    designs = designs.Where(d => d.StructureScore < filterValue || d.StructureScore > upperBound);
                    break;
                default:
                    break;
            }
        }

        /*! \fn DownloadFiles
         * \brief Function to perform the file creation
         * \param designs List of designs
         * \param obj List of selected designs, if needed
         * \param format File format (ie. CSV, FASTA, ZIP)
         * \param stream Output memory stream
         * \param extension File extension (ie. .csv, .fasta, .zip)
         * \param type File type
         */
        private void DownloadFiles(IQueryable<Design> designs, JObject obj, string format, out MemoryStream stream, out string extension, out string type)
        {
            var payload = "";
            stream = new MemoryStream();
            byte[] byteArray;
            switch (format)
            {
                case "csv":
                    payload += String.Format("Rank,DesiredTemperatureScore,SpecificityScore,AccessibilityScore,StructureScore,CreatedAt,UpdatedAt,Sequence\n");
                    extension = "csv";
                    foreach (Design d in designs)
                    {
                        if (obj == null || obj.ContainsKey(d.JobId.ToString() + '-' + d.Id.ToString()))
                        {
                            payload += String.Format("{0},{1},{2},{3},{4},{5},{6},{7}\n", d.Rank, d.DesiredTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence);
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
                        if (obj == null || obj.ContainsKey(d.JobId.ToString() + '-' + d.Id.ToString()))
                        {
                            AddToZip(ref zipStream, d);
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
                        if (obj == null || obj.ContainsKey(d.JobId.ToString() + '-' + d.Id.ToString()))
                        {
                            payload += String.Format(">Rank {0} | DesiredTemperatureScore {1} | SpecificityScore {2} | AccessibilityScore {3} | StructureScore {4} | CreatedAt {5} | UpdatedAt {6}\n{7}\n\n", d.Rank, d.DesiredTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence);
                        }
                    }
                    type = "text/plain";
                    byteArray = Encoding.ASCII.GetBytes(payload);
                    stream = new MemoryStream(byteArray);
                    break;
                default:
                    extension = "";
                    type = "";
                    break;
            }
        }

        /*! \fn AddToZip
         * \brief Helper function to add design to ZIP
         * \param zipStream Output stream for zip
         * \param d Design to add
         */
        private void AddToZip(ref ZipOutputStream zipStream, Design d)
        {
            byte[] byteArray;
            var newEntry = new ZipEntry("design" + d.Id + ".fasta");
            newEntry.DateTime = DateTime.Now;

            zipStream.PutNextEntry(newEntry);
            byteArray = Encoding.ASCII.GetBytes(String.Format(">Rank {0} | DesiredTemperatureScore {1} | SpecificityScore {2} | AccessibilityScore {3} | StructureScore {4} | CreatedAt {5} | UpdatedAt {6}\n{7}\n\n", d.Rank, d.DesiredTemperatureScore, d.SpecificityScore, d.AccessibilityScore, d.StructureScore, d.CreatedAt, d.UpdatedAt, d.Sequence));

            MemoryStream inStream = new MemoryStream(byteArray);
            StreamUtils.Copy(inStream, zipStream, new byte[4096]);
            inStream.Close();
            zipStream.CloseEntry();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Ribosoft.Models;
using Ribosoft.GenbankRequests;
using Ribosoft.Jobs;
using Ribosoft.Data;

namespace Ribosoft.Controllers
{
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public RequestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
            ViewData["Assemblies"] = _context.Assemblies
                .OrderBy(a => a.OrganismName)
                .Select(a => new SelectListItem{ Value = a.TaxonomyId.ToString(), Text = string.Format("{0} (taxon {1})", a.OrganismName, a.TaxonomyId) });
            
            return View(new RequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> Index(RequestViewModel model)
        {
            this.ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
            var user = await GetUser();
            Job job = new Job();
            
            if (ModelState.IsValid) {
                _context.Add(job);
                
                job.RibozymeId = model.RibozymeStructure;
                job.RNAInput = model.InputSequence;
                job.Temperature = model.Temperature;
                job.Na = model.Na;
                job.Probe = model.Probe;
                job.FivePrime = model.TargetRegions.Any(tr => tr.Id == 1 && tr.Selected);
                job.OpenReadingFrame = model.TargetRegions.Any(tr => tr.Id == 2 && tr.Selected);
                job.ThreePrime = model.TargetRegions.Any(tr => tr.Id == 3 && tr.Selected);
                job.OpenReadingFrameStart = model.OpenReadingFrameStart;
                job.OpenReadingFrameEnd = model.OpenReadingFrameEnd;
                job.OwnerId = user.Id;
                job.JobState = JobState.New;

                if (model.SelectedTargetEnvironment == TargetEnvironment.InVivo && model.InVivoEnvironment.HasValue)
                {
                    job.TargetEnvironment = TargetEnvironment.InVivo;
                    job.SpecificityMethod = model.SelectedSpecificityMethod;
                    job.AssemblyId = model.InVivoEnvironment.Value;
                }
                else
                {
                    job.TargetEnvironment = TargetEnvironment.InVitro;
                }
                                
                await _context.SaveChangesAsync();
                
                job.HangfireJobId = BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase1(job.Id, JobCancellationToken.Null));

                await _context.SaveChangesAsync();                

                return RedirectToAction("Details", "Jobs", new { id = job.Id });
            }
            
            ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
            ViewData["Assemblies"] = _context.Assemblies
                .OrderBy(a => a.OrganismName)
                .Select(a => new SelectListItem{ Value = a.TaxonomyId.ToString(), Text = string.Format("{0} (taxon {1})", a.OrganismName, a.TaxonomyId)});

            return View(model);
        }

        [HttpGet]
        public string GetSequenceFromGenbank(string accession)
        {
            return GenbankRequest.RunSequenceRequest(accession);
        }

        [HttpGet]
        public string GetStartIndexFromGenbank(string accession)
        {
            return GenbankRequest.RunStartIndexRequest(accession);
        }

        [HttpGet]
        public string GetEndIndexFromGenbank(string accession)
        {
            return GenbankRequest.RunEndIndexRequest(accession);
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
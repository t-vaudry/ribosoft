using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public RequestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
            return View(new RequestViewModel
            {
                TargetRegions = new TargetRegion[] {
                    new TargetRegion(1, "5'UTR", false),
                    new TargetRegion(2, "Open Reading Frame (ORF)", false),
                    new TargetRegion(3, "3'UTR", false)
                },
                TargetEnvironment = new TargetEnvironmentRadioInput(),
                Specificity = new SpecificityRadioInput()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> Index(RequestViewModel model)
        {
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
                await _context.SaveChangesAsync();

                job.HangfireJobId = BackgroundJob.Enqueue<GenerateCandidates>(x => x.Generate(job.Id, JobCancellationToken.Null));
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

                //return Content($"Structure: {model.RibozymeStructure}\nSequence: {model.InputSequence}\nTest: {model.TargetRegions[0].Selected}");
            }
            
            ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ribosoft.Models;
using Ribosoft.GenbankRequests;
using Ribosoft.Jobs;
using Ribosoft.Data;
using Ribosoft.Models.RequestViewModels;

namespace Ribosoft.Controllers
{
    /*! \class RequestController
     * \brief Controller class for the request page
     */
    public class RequestController : Controller
    {
        /*! \property _context
         * \brief Local application database context
         */
        private readonly ApplicationDbContext _context;

        /*! \property _userManager
         * \brief Manager of application users
         */
        private readonly UserManager<ApplicationUser> _userManager;

        /*! \property _configuration
         * \brief Local application configuration
         */
        private readonly IConfiguration _configuration;

        /*! \fn RequestController
         * \brief Default constructor
         * \param context Application database context
         * \param userManager Application user manager
         * \param configuration Application configuration
         */
        public RequestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        /*!
         * \brief HTTP GET request for request page
         * \return View of the request index
         */
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetUser();

            ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
            ViewData["Assemblies"] = _context.Assemblies
                .Where(a => a.IsEnabled)
                .OrderBy(a => a.OrganismName)
                .Select(a => new SelectListItem {Value = a.TaxonomyId.ToString(), Text = string.Format("{0} (taxon {1})", a.OrganismName, a.TaxonomyId)});

            var viewModel = new RequestViewModel
            {
                ExceededMaxRequests = await ExceededMaxRequests(user)
            };

            return View(viewModel);
        }

        /*!
         * \brief HTTP POST request to submit request form
         * \param model Request view model object, containing information pertaining to submitted request
         * \return View based on results
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RequestViewModel model)
        {
            var user = await GetUser();

            model.ExceededMaxRequests = await ExceededMaxRequests(user);

            if (ModelState.IsValid && !model.ExceededMaxRequests)
            {
                var job = new Job();
                _context.Add(job);

                job.RibozymeId = model.RibozymeStructure;
                job.RNAInput = model.InputSequence.Replace('T','U');
                job.Temperature = model.Temperature;
                job.Na = model.Na;
                job.Probe = model.Probe;
                job.DesiredTempTolerance = model.DesiredTemperatureTolerance;
                job.HighestTempTolerance = model.HighestTemperatureTolerance;
                job.SpecificityTolerance = model.SpecificityTolerance;
                job.AccessibilityTolerance = model.AccessibilityTolerance;
                job.StructureTolerance = model.StructureTolerance;
                job.MalformationTolerance = model.MalformationTolerance;
                job.FivePrime = model.TargetRegions.Any(tr => tr.Id == 1 && tr.Selected);
                job.OpenReadingFrame = model.TargetRegions.Any(tr => tr.Id == 2 && tr.Selected);
                job.ThreePrime = model.TargetRegions.Any(tr => tr.Id == 3 && tr.Selected);
                job.OpenReadingFrameStart = model.OpenReadingFrameStart;
                job.OpenReadingFrameEnd = model.OpenReadingFrameEnd;
                job.OwnerId = user.Id;
                job.JobState = JobState.New;

                if ((model.SnakeSequence.Length <= (model.LowerStemIILength + model.BulgeLength + model.UpperStemIILength + model.LoopLength)) &&
                    (model.SnakeSequence.Length > (model.LowerStemIILength + model.BulgeLength + model.UpperStemIILength)))
                    job.SnakeSequence = model.SnakeSequence.Replace('T', 'U');
                else
                    job.SnakeSequence = "";

                job.StemITemperature = model.StemITemperature;
                job.StemIIITemperature = model.StemIIITemperature;
                job.LowerStemIILength = model.LowerStemIILength;
                job.BulgeLength = model.BulgeLength;
                job.UpperStemIILength = model.UpperStemIILength;
                job.LoopLength = model.LoopLength;

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

                return RedirectToAction("Details", "Jobs", new {id = job.Id});
            }

            ViewData["Ribozymes"] = new SelectList(_context.Ribozymes, "Id", "Name");
            ViewData["Assemblies"] = _context.Assemblies
                .Where(a => a.IsEnabled)
                .OrderBy(a => a.OrganismName)
                .Select(a => new SelectListItem {Value = a.TaxonomyId.ToString(), Text = string.Format("{0} (taxon {1})", a.OrganismName, a.TaxonomyId)});

            return View(model);
        }

        /*! \fn GetSequenceFromGenbank
         * \brief HTTP GET request to retrieve RNA sequence from GenBank
         * \param accession Accession number
         * \return JSON result with RNA sequence, open reading frame indices
         */
        [HttpGet]
        public async Task<JsonResult> GetSequenceFromGenbank(string accession)
        {
            IDictionary<string, object> response = new Dictionary<string, object>();

            try
            {
                var result = await GenbankRequest.RunSequenceRequest(accession);
                response["result"] = result;
            }
            catch (GenbankRequestsException e)
            {
                response["error"] = e.Message;
            }
            catch (Exception)
            {
                response["error"] = "An unknown error occurred. Please try again later.";
            }

            return Json(response);
        }

        /*! \fn GetUser
         * \brief Helper function to retrieve current user
         * \return Current user
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

        /*! \fn ExceededMaxRequests
         * \brief Helper function to determine if user has exceeded the maximum number of requests
         * \param user Current user
         * \return Boolean result of check
         */
        private async Task<bool> ExceededMaxRequests(ApplicationUser user)
        {
            return await _context.Jobs.CountAsync(j => j.OwnerId == user.Id) >= 20;
        }
    }
}
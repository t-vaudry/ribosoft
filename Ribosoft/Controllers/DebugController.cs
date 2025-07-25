using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ribosoft.Jobs;
using System.Threading.Tasks;

namespace Ribosoft.Controllers
{
    /*! \class DebugController
     * \brief Controller for debugging job processing issues
     */
    [Authorize]
    public class DebugController : Controller
    {
        private readonly GenerateCandidates _generateCandidates;

        public DebugController(GenerateCandidates generateCandidates)
        {
            _generateCandidates = generateCandidates;
        }

        /*! \fn DiagnoseJob
         * \brief Diagnose why a job is stuck in Structure state
         * \param jobId Job ID to diagnose
         * \return JSON result with diagnosis
         */
        [HttpPost]
        public async Task<IActionResult> DiagnoseJob(int jobId)
        {
            try
            {
                await _generateCandidates.DiagnoseJobTransition(jobId);
                return Json(new { success = true, message = $"Diagnosis completed for job {jobId}. Check application logs for details." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /*! \fn RestartJob
         * \brief Manually restart a stuck job
         * \param jobId Job ID to restart
         * \return JSON result with restart status
         */
        [HttpPost]
        public async Task<IActionResult> RestartJob(int jobId)
        {
            try
            {
                await _generateCandidates.RestartStuckJob(jobId);
                return Json(new { success = true, message = $"Job {jobId} restart attempted. Check job status and logs." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}

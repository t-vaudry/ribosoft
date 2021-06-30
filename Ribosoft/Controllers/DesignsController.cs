using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;

using System.IO;
using System.Text;

namespace Ribosoft.Controllers
{
    /*! \class DesignsController
     * \brief Controller class for the designs of a ribozyme
     */
    public class DesignsController : Controller
    {
        /*! \property _context
         * \brief Local application database context
         */
        private readonly ApplicationDbContext _context;

        /*! \fn DesignsController
         * \brief Default constructor
         * \param context Application database context
         */
        public DesignsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*! \fn Details
         * \brief HTTP GET for view of details of a design
         * \param id Design id
         * \return View of details from design, if found
         */
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var design = await _context.Designs
                .Include(d => d.Job)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (design == null)
            {
                return NotFound();
            }

            return View(design);
        }

        /*! \fn DownloadDesign
         * \brief HTTP GET to download FASTA file of current design
         * \param jobID Job ID, used for filename
         * \param rank Design rank
         * \param desiredTemperatureScore Design desired temperature score
         * \param specificityScore Design specificity score
         * \param accessibilityScore Design accessibility score
         * \param structureScore Design structure score
         * \param createdAt Design created at time
         * \param updatedAt Design updated at time
         * \param sequence Design ribozyme sequence
         * \return FASTA file
         */
        public FileStreamResult DownloadDesign(string jobID, string rank, string desiredTemperatureScore, string specificityScore, string accessibilityScore, string structureScore, string createdAt, string updatedAt, string sequence)
        {
            // TODO: Break up sequence into chunks of max line length
            var payload = String.Format(">Rank {0} | DesiredTemperatureScore {1} | SpecificityScore {2} | AccessibilityScore {3} | StructureScore {4} | CreatedAt {5} | UpdatedAt {6}\n{7}", rank, desiredTemperatureScore, specificityScore, accessibilityScore, structureScore, createdAt, updatedAt, sequence);

            var byteArray = Encoding.ASCII.GetBytes(payload);
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/plain", String.Format("job{0}_rank{1}.fasta", jobID, rank));
        }
    }
}

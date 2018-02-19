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
    public class DesignsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Designs/Details/5
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

        public FileStreamResult DownloadFile(string jobID, string rank, string desiredTemperatureScore, string highestTemperatureScore, string specificityScore, string accessibilityScore, string structureScore, string createdAt, string updatedAt, string sequence)
        {
            // TODO: Break up sequence into chunks of max line length
            var payload = String.Format(">Rank {0} | DesiredTemperatureScore {1} | DesiredTemperatureScore {2} | SpecificityScore {3} | AccessibilityScore {4} | StructureScore {5} | CreatedAt {6} | UpdatedAt {7}\n{8}", rank, desiredTemperatureScore, highestTemperatureScore, specificityScore, accessibilityScore, structureScore, createdAt, updatedAt, sequence);

            var byteArray = Encoding.ASCII.GetBytes(payload);
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/plain", String.Format("job{0}_rank{1}.fasta", jobID, rank));
        }
    }
}

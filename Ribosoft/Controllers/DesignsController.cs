using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;

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
    }
}

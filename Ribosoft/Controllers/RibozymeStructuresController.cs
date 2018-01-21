using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;

namespace Ribosoft.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RibozymeStructuresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RibozymeStructuresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RibozymeStructures
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RibozymeStructures.Include(r => r.Ribozyme);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: RibozymeStructures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ribozymeStructure = await _context.RibozymeStructures
                .Include(r => r.Ribozyme)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (ribozymeStructure == null)
            {
                return NotFound();
            }

            return View(ribozymeStructure);
        }

        // GET: RibozymeStructures/Create
        public IActionResult Create()
        {
            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name");
            return View();
        }

        // POST: RibozymeStructures/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RibozymeId,Cutsite,Sequence,Structure,SubstrateTemplate,SubstrateStructure,PostProcess")] RibozymeStructure ribozymeStructure)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ribozymeStructure);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Ribozymes", new {id = ribozymeStructure.RibozymeId});
            }
            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name", ribozymeStructure.RibozymeId);
            return View(ribozymeStructure);
        }

        // GET: RibozymeStructures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ribozymeStructure = await _context.RibozymeStructures.SingleOrDefaultAsync(m => m.Id == id);
            if (ribozymeStructure == null)
            {
                return NotFound();
            }
            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name", ribozymeStructure.RibozymeId);
            return View(ribozymeStructure);
        }

        // POST: RibozymeStructures/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RibozymeId,Cutsite,Sequence,Structure,SubstrateTemplate,SubstrateStructure,PostProcess")] RibozymeStructure ribozymeStructure)
        {
            if (id != ribozymeStructure.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ribozymeStructure);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RibozymeStructureExists(ribozymeStructure.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Ribozymes", new { id = ribozymeStructure.RibozymeId });
            }
            ViewData["RibozymeId"] = new SelectList(_context.Ribozymes, "Id", "Name", ribozymeStructure.RibozymeId);
            return View(ribozymeStructure);
        }

        // GET: RibozymeStructures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ribozymeStructure = await _context.RibozymeStructures
                .Include(r => r.Ribozyme)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (ribozymeStructure == null)
            {
                return NotFound();
            }

            return View(ribozymeStructure);
        }

        // POST: RibozymeStructures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ribozymeStructure = await _context.RibozymeStructures.SingleOrDefaultAsync(m => m.Id == id);
            _context.RibozymeStructures.Remove(ribozymeStructure);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Ribozymes", new { id = ribozymeStructure.RibozymeId });
        }

        private bool RibozymeStructureExists(int id)
        {
            return _context.RibozymeStructures.Any(e => e.Id == id);
        }
    }
}

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
    public class RibozymesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RibozymesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ribozymes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ribozymes.ToListAsync());
        }

        // GET: Ribozymes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ribozyme = await _context.Ribozymes
                .Include(r => r.RibozymeStructures)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (ribozyme == null)
            {
                return NotFound();
            }

            return View(ribozyme);
        }

        // GET: Ribozymes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ribozymes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Ribozyme ribozyme)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ribozyme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ribozyme);
        }

        // GET: Ribozymes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ribozyme = await _context.Ribozymes.SingleOrDefaultAsync(m => m.Id == id);
            if (ribozyme == null)
            {
                return NotFound();
            }
            return View(ribozyme);
        }

        // POST: Ribozymes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Ribozyme ribozyme)
        {
            if (id != ribozyme.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ribozyme);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RibozymeExists(ribozyme.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ribozyme);
        }

        // GET: Ribozymes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ribozyme = await _context.Ribozymes
                .SingleOrDefaultAsync(m => m.Id == id);
            if (ribozyme == null)
            {
                return NotFound();
            }

            return View(ribozyme);
        }

        // POST: Ribozymes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ribozyme = await _context.Ribozymes.SingleOrDefaultAsync(m => m.Id == id);
            _context.Ribozymes.Remove(ribozyme);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RibozymeExists(int id)
        {
            return _context.Ribozymes.Any(e => e.Id == id);
        }
    }
}

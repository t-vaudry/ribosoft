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
using Ribosoft.Models.RibozymeViewModel;

namespace Ribosoft.Controllers
{
    /*! \class RibozymesController
     * \brief Controller class for the ribozyme templates in the system
     */
    public class RibozymesController : Controller
    {
        /*! \property _context
         * \brief Local application database context
         */
        private readonly ApplicationDbContext _context;

        /*! \fn RibozymesController
         * \brief Default constructor
         * \param context Application database context
         */
        public RibozymesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*! \fn Index
         * \brief HTTP GET request for ribozymes page
         * \return View of ribozymes index
         */
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ribozymes.ToListAsync());
        }

        /*! \fn Details
         * \brief HTTP GET request for details page
         * \return View of details index
         */
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

        /*!
         * \brief HTTP GET request for create page
         * \return View of create ribozyme index
         */
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        /*!
         * \brief HTTP POST request to create new ribozyme
         * \param model Model containing ribozyme creation information
         * \return View of resulting ribozyme details index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(RibozymeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ribozyme = new Ribozyme
                {
                    Name = model.Name
                };
                
                _context.Add(ribozyme);
                await _context.SaveChangesAsync();
                
                var ribozymeStructure = new RibozymeStructure
                {
                    RibozymeId = ribozyme.Id,
                    Sequence = model.Sequence,
                    Structure = model.Structure,
                    SubstrateTemplate = model.SubstrateTemplate,
                    SubstrateStructure = model.SubstrateStructure,
                    Cutsite = model.Cutsite,
                    PostProcess = model.PostProcess
                };
                
                _context.Add(ribozymeStructure);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Details), "Ribozymes", new {id = ribozyme.Id});
            }
            
            return View(model);
        }

        /*!
         * \brief HTTP GET request for edit ribozyme page
         * \param id Ribozyme Id
         * \return View of edit ribozyme index
         */
        [Authorize(Roles = "Administrator")]
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

        /*!
         * \brief HTTP POST request for edit ribozyme form submission
         * \param id Ribozyme Id
         * \param ribozyme Ribozyme object
         * \return View of edited ribozyme details index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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
                return RedirectToAction(nameof(Details), "Ribozymes", new { id });
            }
            return View(ribozyme);
        }

        /*! \fn Delete
         * \brief HTTP GET request for deleting a ribozyme
         * \param id Ribozyme Id
         * \return View of ribozyme index
         */
        [Authorize(Roles = "Administrator")]
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
        /*! \fn DeleteConfirmed
         * \brief HTTP POST request to confirm delete ribozyme
         * \param id Ribozyme Id
         * \return View of ribozyme index
         */
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ribozyme = await _context.Ribozymes.SingleOrDefaultAsync(m => m.Id == id);
            _context.Ribozymes.Remove(ribozyme);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /*! \fn RibozymeExists
         * \brief Helper function to check for existence of ribozyme
         * \param id Ribozyme Id
         * \return Boolean result of the check
         */
        private bool RibozymeExists(int id)
        {
            return _context.Ribozymes.Any(e => e.Id == id);
        }
    }
}

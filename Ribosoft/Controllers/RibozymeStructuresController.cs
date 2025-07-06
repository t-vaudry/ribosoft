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
    /*! \class RibozymeStructuresController
     * \brief Controller class for the ribozyme structures within the system
     */
    public class RibozymeStructuresController : Controller
    {
        /*! \property _context
         * \brief Local application database context
         */
        private readonly ApplicationDbContext _context;

        /*! \fn RibozymeStructuresController
         * \brief Default constructor
         * \param context Application database context
         */
        public RibozymeStructuresController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*! \fn Index
         * \brief HTTP GET request for ribozyme structures page
         * \return View of ribozyme structures index
         */
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RibozymeStructures.Include(r => r.Ribozyme);
            return View(await applicationDbContext.ToListAsync());
        }

        /*! \fn Details
         * \brief HTTP GET request for ribozyme structures details page
         * \param id RibozymeStructure Id
         * \return View of ribozyme structures details index
         */
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

        /*!
         * \brief HTTP GET request for ribozyme structures create page
         * \param ribozymeId Ribozyme Id
         * \return View of ribozyme structures create index
         */
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(int? ribozymeId)
        {
            if (ribozymeId == null)
            {
                return NotFound();
            }
            
            var ribozyme = await _context.Ribozymes.SingleOrDefaultAsync(m => m.Id == ribozymeId);
            if (ribozyme == null)
            {
                return NotFound();
            }
            
            ViewData["RibozymeId"] = ribozyme.Id;
            return View();
        }

        /*!
         * \brief HTTP POST request for ribozyme structures create form submission
         * \param ribozymeId Ribozyme Id
         * \param ribozymeStructure Ribozyme Structure details for creation
         * \return View of ribozyme structures details index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(int? ribozymeId, [Bind("Cutsite,Sequence,Structure,SubstrateTemplate,SubstrateStructure,PostProcess")] RibozymeStructure ribozymeStructure)
        {
            if (ribozymeId == null)
            {
                return NotFound();
            }
            
            var ribozyme = await _context.Ribozymes.SingleOrDefaultAsync(m => m.Id == ribozymeId);
            if (ribozyme == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                ribozymeStructure.RibozymeId = ribozyme.Id;
                _context.Add(ribozymeStructure);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Ribozymes", new {id = ribozymeStructure.RibozymeId});
            }

            ViewData["RibozymeId"] = ribozyme.Id;
            return View(ribozymeStructure);
        }

        /*!
         * \brief HTTP GET request for ribozyme structures edit page
         * \param id Ribozyme Structure Id
         * \return View of ribozyme structures edit index
         */
        [Authorize(Roles = "Administrator")]
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
            
            return View(ribozymeStructure);
        }

        /*!
         * \brief HTTP POST request for ribozyme structures edit form submission
         * \param id Ribozyme Structure Id
         * \param ribozymeStructure Ribozyme structure details for editing
         * \return View of ribozyme structures details index
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
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
            
            return View(ribozymeStructure);
        }

        /*! \fn Delete
         * \brief HTTP GET request for ribozyme structures delete page
         * \param id Ribozyme Structure Id
         * \return View of ribozyme structures delete index
         */
        [Authorize(Roles = "Administrator")]
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

        /*! \fn DeleteConfirmed
         * \brief HTTP POST request to delete ribozyme structure
         * \param id Ribozyme Structure Id
         * \return View of ribozyme structures details index
         */
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ribozymeStructure = await _context.RibozymeStructures.SingleOrDefaultAsync(m => m.Id == id);
            if (ribozymeStructure != null)
            {
                _context.RibozymeStructures.Remove(ribozymeStructure);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Ribozymes", new { id = ribozymeStructure.RibozymeId });
            }
            return NotFound();
        }

        /*! \fn RibozymeStructureExists
         * \brief Helper function to determine if ribozyme structure exists
         * \param id Ribozyme Structure Id
         * \return Boolean result of the check
         */
        private bool RibozymeStructureExists(int? id)
        {
            return _context.RibozymeStructures.Any(e => e.Id == id);
        }
    }
}

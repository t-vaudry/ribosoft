using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.RibozymeViewModel
{
    /*! \class RibozymeCreateViewModel
     * \brief Model class for the create ribozyme view
     */
    [ValidationAttributes.ValidateRibozymeStructure]
    public class RibozymeCreateViewModel
    {
        /*! \fn RibozymeCreateViewModel
         * \brief Default constructor
         */
        public RibozymeCreateViewModel()
        {
            RibozymeStructures = new List<RibozymeStructure>();
        }

        /*! \property Name
         * \brief Ribozyme name
         */
        [Required]
        [Display(Name = "Ribozyme Name")]
        public string Name { get; set; }

        /*! \property RibozymeStructures
         * \brief List of ribozyme structures
         */
        [Required]
        public List<RibozymeStructure> RibozymeStructures { get; set; }
    }
}
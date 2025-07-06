using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.RequestViewModels
{
	/*! \class SpecificityMethodViewModel
     * \brief Model class for the specificity method view
     */
    public class SpecificityMethodViewModel
    {
    	/*! \property Name
         * \brief Name of method
         */
        public string Name { get; set; } = string.Empty;

        /*! \property Value
         * \brief Specificity method value
         */
        public SpecificityMethod Value { get; set; }
    }
}

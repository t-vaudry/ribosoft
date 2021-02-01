using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.RequestViewModels
{
	/*! \class TargetEnvironmentViewModel
     * \brief Model class for the target environment view
     */
    public class TargetEnvironmentViewModel
    {
    	/*! \property Name
         * \brief Target environment name
         */
        public string Name { get; set; }

        /*! \property Value
         * \brief Target environment value
         */
        public TargetEnvironment Value { get; set; }
    }
}

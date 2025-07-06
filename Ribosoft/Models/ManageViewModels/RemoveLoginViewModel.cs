using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
	/*! \class RemoveLoginViewModel
     * \brief Model class for the remove login view
     */
    public class RemoveLoginViewModel
    {
    	/*! \property LoginProvider
         * \brief External login provider
         */
        public string LoginProvider { get; set; } = string.Empty;

        /*! \property ProviderKey
         * \brief External provider key
         */
        public string ProviderKey { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Models.ManageViewModels
{
    /*! \class ShowRecoveryCodesViewModel
     * \brief Model class for recovery codes of your account
     */
    public class ShowRecoveryCodesViewModel
    {
    	/*! \property RecoveryCodes
         * \brief Array of recovery codes
         */
        public string[] RecoveryCodes { get; set; }
    }
}

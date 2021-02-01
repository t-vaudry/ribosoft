using System;

//! \namespace  Ribosoft.Models
namespace Ribosoft.Models
{
    /*! \class ErrorViewModel
     * \brief Model for the error view.
     */
    public class ErrorViewModel
    {
    	/*! \property RequestId
         * \brief Request ID
         */
        public string RequestId { get; set; }

        /*! \property ShowRequestId
         * \brief Check if request ID is displayed
         */
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
using System.Collections.Generic;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ribosoft.Models.JobsViewModels
{
    /*! \class JobIndexViewModel
     * \brief Model class for the job index view
     */
    public class JobIndexViewModel
    {
        /*! \property InProgress
         * \brief List of in-progress jobs
         */
        public IEnumerable<Job> InProgress { get; set; }

        /*! \property Completed
         * \brief Paged result of completed jobs
         */
        public PagedResult<Job> Completed { get; set; }

        /*! \property JobId
         * \brief Job ID to transfer ownership
         */
        public string JobId { get; set; }

        /*! \property ErrorMessages
         * \brief List of Error message strings
         */
        public List<string> ErrorMessages { get; set; }

        /*! \property SuccessMessages
         * \brief List of Success message strings
         */
        public List<string> SuccessMessages { get; set; }

        /*! \property UploadFile
         * \brief Holder for uploaded jobs file
         */
        [BindProperty]
        public IFormFile UploadFile { get; set; }

        /*! \fn JobIndexViewModel
         * \brief Default constructor
         */
        public JobIndexViewModel()
        {
            InProgress = new List<Job>();
            Completed = new PagedResult<Job>();
            ErrorMessages = new List<string>();
            SuccessMessages = new List<string>();
        }
    }
}

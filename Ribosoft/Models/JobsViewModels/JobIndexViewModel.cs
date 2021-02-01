using System.Collections.Generic;
using cloudscribe.Pagination.Models;

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

        /*! \property ErrorMessage
         * \brief Error message string
         */
        public string ErrorMessage { get; set; }

        /*! \property SuccessMessage
         * \brief Success message string
         */
        public string SuccessMessage { get; set; }

        /*! \fn JobIndexViewModel
         * \brief Default constructor
         */
        public JobIndexViewModel()
        {
            InProgress = new List<Job>();
            Completed = new PagedResult<Job>();
        }
    }
}

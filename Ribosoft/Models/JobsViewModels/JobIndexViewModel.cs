using System.Collections.Generic;
using cloudscribe.Pagination.Models;

namespace Ribosoft.Models.JobsViewModels
{
    public class JobIndexViewModel
    {
        public IEnumerable<Job> InProgress { get; set; }
        public PagedResult<Job> Completed { get; set; }
        public string JobId { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public JobIndexViewModel()
        {
            InProgress = new List<Job>();
            Completed = new PagedResult<Job>();
        }
    }
}

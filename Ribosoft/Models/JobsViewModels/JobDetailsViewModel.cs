using System;
using cloudscribe.Pagination.Models;

namespace Ribosoft.Models.JobsViewModels
{
    public class JobDetailsViewModel
    {
        public Job Job { get; set; }
        public PagedResult<Design> Designs { get; set; }
        public string SortOrder { get; set; } = "";

        public JobDetailsViewModel()
        {
            Designs = new PagedResult<Design>();
        }
    }
}

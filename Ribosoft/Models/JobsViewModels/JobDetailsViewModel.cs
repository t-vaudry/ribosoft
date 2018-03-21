using System;
using System.Collections.Generic;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ribosoft.Models.JobsViewModels
{
    public class JobDetailsViewModel
    {
        public Job Job { get; set; }
        public PagedResult<Design> Designs { get; set; }
        public string SortOrder { get; set; } = "";
        public IEnumerable<SelectListItem> FilterParams { get; set; }
        public IEnumerable<SelectListItem> FilterConditions { get; set; }
        public string FilterParam { get; set; }
        public string FilterCondition { get; set; }
        public string FilterValue { get; set; }

        public JobDetailsViewModel()
        {
            Designs = new PagedResult<Design>();
        }
    }
}

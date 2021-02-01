using System;
using System.Collections.Generic;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ribosoft.Models.JobsViewModels
{
    /*! \class JobDetailsViewModel
     * \brief Model class for the job details view
     */
    public class JobDetailsViewModel
    {
        /*! \property Job
         * \brief Current job
         */
        public Job Job { get; set; }

        /*! \property Designs
         * \brief Paged result of designs
         */
        public PagedResult<Design> Designs { get; set; }

        /*! \property SortOrder
         * \brief Current sort order
         */
        public string SortOrder { get; set; } = "";

        /*! \property FilterParams
         * \brief List of available filter parameters
         */
        public IEnumerable<SelectListItem> FilterParams { get; set; }

        /*! \property FilterConditions
         * \brief List of available filter conditions
         */
        public IEnumerable<SelectListItem> FilterConditions { get; set; }

        /*! \property FilterParam
         * \brief Current value of filter parameter
         */
        public string FilterParam { get; set; }

        /*! \property FilterCondition
         * \brief Current value of filter condition
         */
        public string FilterCondition { get; set; }

        /*! \property FilterValue
         * \brief Current value of filter value
         */
        public string FilterValue { get; set; }

        /*! \fn JobDetailsViewModel
         * \brief Default constructor
         */
        public JobDetailsViewModel()
        {
            Designs = new PagedResult<Design>();
        }
    }
}

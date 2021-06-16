using System;
using System.Collections.Generic;
using cloudscribe.Pagination.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace Ribosoft.Models.JobsViewModels
{
    /*! \class JobDetailsViewModel
     * \brief Model class for the job details view
     */
    public class JobDetailsViewModel
    {
        /*! \struct Filter
         * \brief Filter structure to define a filter
         */
        public struct Filter
        {
            /*! \property param
             * \brief Filter parameter
             */
            public string param;

            /*! \property condition
             * \brief Filter condition
             */
            public string condition;

            /*! \property value
             * \brief Filter value
             */
            public string value;

            /*! \fn GetJson
             * \brief Helper function to generate the JSON string for filtering
             * \return JSON string of filter data
             */
            public string GetJson()
            {
                JObject json = new JObject();
                json["field"] = new JObject(new JProperty("label", GetLabel(param)), new JProperty("value", param));
                json["operator"] = new JObject(new JProperty("label", GetLabel(condition)), new JProperty("value", condition));
                json["value"] = new JObject(new JProperty("label", value), new JProperty("value", value));
                return json.ToString();
            }

            /*! \fn GetLabel
             * \brief Get label string from parameter id
             * \param param Parameter ID
             * \return Label str
             */
            private string GetLabel(string param)
            {
                switch(param)
                {
                    case "Rank":
                        return "Rank";
                    case "HighestTemperatureScore":
                        return "Highest Temperature Score";
                    case "DesiredTemperatureScore":
                        return "Desired Temperature Score";
                    case "AccessibilityScore":
                        return "Accessibility Score";
                    case "SpecificityScore":
                        return "Specificity Score";
                    case "StructureScore":
                        return "Structure Score";
                    case "MalformationScore":
                        return "Malformation Score";
                    case "eq":
                        return "=";
                    case "ne":
                        return "!=";
                    case "gt":
                        return ">";
                    case "lt":
                        return "<";
                    default:
                        return "";
                }
            }
        }

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

        /*! \property FilterList
         * \brief List of filter elements
         */
        public List<Filter> FilterList { get; set; }

        /*! \fn JobDetailsViewModel
         * \brief Default constructor
         */
        public JobDetailsViewModel()
        {
            Designs = new PagedResult<Design>();
            FilterList = new List<Filter>();
        }
    }
}

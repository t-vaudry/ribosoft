using System.Collections.Generic;
using NCBI.Datasets.API.Models.Common;

namespace NCBI.Datasets.API.Models.Responses
{
    /// <summary>
    /// Response model for BioSample data report operations
    /// </summary>
    public class BioSampleDataReportResponse
    {
        /// <summary>
        /// List of BioSample data reports
        /// </summary>
        public List<BioSampleDataReport> Reports { get; set; } = new List<BioSampleDataReport>();

        /// <summary>
        /// Total count of available reports
        /// </summary>
        public int? TotalCount { get; set; }

        /// <summary>
        /// Next page token for pagination
        /// </summary>
        public string? NextPageToken { get; set; }
    }

    /// <summary>
    /// Individual BioSample data report
    /// </summary>
    public class BioSampleDataReport
    {
        /// <summary>
        /// BioSample accession
        /// </summary>
        public string? Accession { get; set; }

        /// <summary>
        /// Sample name
        /// </summary>
        public string? SampleName { get; set; }

        /// <summary>
        /// Organism information
        /// </summary>
        public BioSampleOrganismInfo? Organism { get; set; }

        /// <summary>
        /// Sample attributes
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Collection date
        /// </summary>
        public string? CollectionDate { get; set; }

        /// <summary>
        /// Geographic location
        /// </summary>
        public string? GeographicLocation { get; set; }

        /// <summary>
        /// Host information
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Isolation source
        /// </summary>
        public string? IsolationSource { get; set; }

        /// <summary>
        /// Sample type
        /// </summary>
        public string? SampleType { get; set; }

        /// <summary>
        /// Tissue type
        /// </summary>
        public string? Tissue { get; set; }

        /// <summary>
        /// Strain information
        /// </summary>
        public string? Strain { get; set; }

        /// <summary>
        /// Cultivar information
        /// </summary>
        public string? Cultivar { get; set; }

        /// <summary>
        /// Serotype information
        /// </summary>
        public string? Serotype { get; set; }

        /// <summary>
        /// Sub-species information
        /// </summary>
        public string? SubSpecies { get; set; }

        /// <summary>
        /// Submission date
        /// </summary>
        public string? SubmissionDate { get; set; }

        /// <summary>
        /// Last update date
        /// </summary>
        public string? LastUpdateDate { get; set; }

        /// <summary>
        /// Publication date
        /// </summary>
        public string? PublicationDate { get; set; }

        /// <summary>
        /// Owner information
        /// </summary>
        public BioSampleOwnerInfo? Owner { get; set; }
    }

    /// <summary>
    /// BioSample organism information
    /// </summary>
    public class BioSampleOrganismInfo
    {
        /// <summary>
        /// Taxonomy ID
        /// </summary>
        public int? TaxId { get; set; }

        /// <summary>
        /// Scientific name
        /// </summary>
        public string? ScientificName { get; set; }

        /// <summary>
        /// Common name
        /// </summary>
        public string? CommonName { get; set; }

        /// <summary>
        /// Taxonomic rank
        /// </summary>
        public RankType? Rank { get; set; }
    }

    /// <summary>
    /// BioSample owner information
    /// </summary>
    public class BioSampleOwnerInfo
    {
        /// <summary>
        /// Owner name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Owner contacts
        /// </summary>
        public List<BioSampleContact> Contacts { get; set; } = new List<BioSampleContact>();
    }

    /// <summary>
    /// BioSample contact information
    /// </summary>
    public class BioSampleContact
    {
        /// <summary>
        /// Contact name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Contact email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Contact affiliation
        /// </summary>
        public string? Affiliation { get; set; }
    }
}

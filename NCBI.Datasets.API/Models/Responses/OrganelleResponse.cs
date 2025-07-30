using System.Collections.Generic;
using NCBI.Datasets.API.Models.Common;
using NCBI.Datasets.API.Models.Enums;

namespace NCBI.Datasets.API.Models.Responses
{
    /// <summary>
    /// Response model for organelle download operations
    /// </summary>
    public class OrganelleDownloadResponse
    {
        /// <summary>
        /// Binary ZIP file data
        /// </summary>
        public byte[]? Data { get; set; }

        /// <summary>
        /// Content type of the response
        /// </summary>
        public string ContentType { get; set; } = "application/zip";

        /// <summary>
        /// Filename for the download
        /// </summary>
        public string Filename { get; set; } = "ncbi_dataset.zip";
    }

    /// <summary>
    /// Response model for organelle data reports
    /// </summary>
    public class OrganelleDataReportsResponse
    {
        /// <summary>
        /// Messages from the API response
        /// </summary>
        public List<ReportsMessage> Messages { get; set; } = new List<ReportsMessage>();

        /// <summary>
        /// List of organelle data reports
        /// </summary>
        public List<OrganelleDataReport> Reports { get; set; } = new List<OrganelleDataReport>();

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
    /// Individual organelle data report
    /// </summary>
    public class OrganelleDataReport
    {
        /// <summary>
        /// Organelle accession
        /// </summary>
        public string? Accession { get; set; }

        /// <summary>
        /// Organelle type (mitochondrion, chloroplast, etc.)
        /// </summary>
        public string? OrganelleType { get; set; }

        /// <summary>
        /// Organism information
        /// </summary>
        public OrganelleOrganismInfo? Organism { get; set; }

        /// <summary>
        /// Assembly information
        /// </summary>
        public OrganelleAssemblyInfo? Assembly { get; set; }

        /// <summary>
        /// BioSample information
        /// </summary>
        public OrganelleBioSample? BioSample { get; set; }

        /// <summary>
        /// Sequence length
        /// </summary>
        public int? SequenceLength { get; set; }

        /// <summary>
        /// GC content percentage
        /// </summary>
        public double? GcContent { get; set; }

        /// <summary>
        /// Gene count
        /// </summary>
        public int? GeneCount { get; set; }

        /// <summary>
        /// Topology (linear or circular)
        /// </summary>
        public OrganelleTopology? Topology { get; set; }

        /// <summary>
        /// Submission date
        /// </summary>
        public string? SubmissionDate { get; set; }

        /// <summary>
        /// Last update date
        /// </summary>
        public string? LastUpdateDate { get; set; }
    }

    /// <summary>
    /// Organelle organism information
    /// </summary>
    public class OrganelleOrganismInfo
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
    /// Organelle assembly information
    /// </summary>
    public class OrganelleAssemblyInfo
    {
        /// <summary>
        /// Assembly accession
        /// </summary>
        public string? Accession { get; set; }

        /// <summary>
        /// Assembly name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Assembly level
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// Assembly status
        /// </summary>
        public string? Status { get; set; }
    }

    /// <summary>
    /// Organelle BioSample information
    /// </summary>
    public class OrganelleBioSample
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
        /// Collection date
        /// </summary>
        public string? CollectionDate { get; set; }

        /// <summary>
        /// Geographic location
        /// </summary>
        public string? GeographicLocation { get; set; }
    }

    /// <summary>
    /// Organelle topology types
    /// </summary>
    public enum OrganelleTopology
    {
        /// <summary>
        /// Linear topology
        /// </summary>
        Linear,

        /// <summary>
        /// Circular topology
        /// </summary>
        Circular,

        /// <summary>
        /// Unknown topology
        /// </summary>
        Unknown
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCBI.Datasets.API.Models.Enums;

namespace NCBI.Datasets.API.Models.Requests
{
    /// <summary>
    /// Request model for prokaryote gene operations by protein accession
    /// </summary>
    public class ProkaryoteGeneRequest
    {
        /// <summary>
        /// List of protein accessions (WP_ prefixed RefSeq protein accessions)
        /// </summary>
        [Required]
        public List<string> Accessions { get; set; } = new List<string>();

        /// <summary>
        /// Annotation types to include in the response
        /// </summary>
        public List<FastaType> IncludeAnnotationType { get; set; } = new List<FastaType>();

        /// <summary>
        /// Gene flank configuration for upstream/downstream sequences
        /// </summary>
        public ProkaryoteGeneRequestGeneFlankConfig? GeneFlankConfig { get; set; }

        /// <summary>
        /// NCBI Taxonomy ID or name to filter results
        /// </summary>
        public string? Taxon { get; set; }
    }

    /// <summary>
    /// Configuration for gene flanking sequences
    /// </summary>
    public class ProkaryoteGeneRequestGeneFlankConfig
    {
        /// <summary>
        /// Length of flanking sequence to include (in base pairs)
        /// </summary>
        public int? Length { get; set; }
    }
}

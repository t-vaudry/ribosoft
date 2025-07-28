using System.Collections.Generic;

namespace NCBI.Datasets.API.Models.Common
{
    /// <summary>
    /// Represents a message from the API response
    /// </summary>
    public class ReportsMessage
    {
        /// <summary>
        /// Message severity level
        /// </summary>
        public string? Severity { get; set; }

        /// <summary>
        /// Message text
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Message code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Additional message details
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a warning from the API response
    /// </summary>
    public class ReportsWarning
    {
        /// <summary>
        /// Warning message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Warning code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Field that caused the warning
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Additional warning details
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents an error from the API response
    /// </summary>
    public class ReportsError
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Field that caused the error
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Additional error details
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Taxonomic rank types
    /// </summary>
    public enum RankType
    {
        /// <summary>
        /// No rank specified
        /// </summary>
        NO_RANK,

        /// <summary>
        /// Superkingdom rank
        /// </summary>
        SUPERKINGDOM,

        /// <summary>
        /// Kingdom rank
        /// </summary>
        KINGDOM,

        /// <summary>
        /// Subkingdom rank
        /// </summary>
        SUBKINGDOM,

        /// <summary>
        /// Superphylum rank
        /// </summary>
        SUPERPHYLUM,

        /// <summary>
        /// Phylum rank
        /// </summary>
        PHYLUM,

        /// <summary>
        /// Subphylum rank
        /// </summary>
        SUBPHYLUM,

        /// <summary>
        /// Superclass rank
        /// </summary>
        SUPERCLASS,

        /// <summary>
        /// Class rank
        /// </summary>
        CLASS,

        /// <summary>
        /// Subclass rank
        /// </summary>
        SUBCLASS,

        /// <summary>
        /// Infraclass rank
        /// </summary>
        INFRACLASS,

        /// <summary>
        /// Cohort rank
        /// </summary>
        COHORT,

        /// <summary>
        /// Subcohort rank
        /// </summary>
        SUBCOHORT,

        /// <summary>
        /// Superorder rank
        /// </summary>
        SUPERORDER,

        /// <summary>
        /// Order rank
        /// </summary>
        ORDER,

        /// <summary>
        /// Suborder rank
        /// </summary>
        SUBORDER,

        /// <summary>
        /// Infraorder rank
        /// </summary>
        INFRAORDER,

        /// <summary>
        /// Parvorder rank
        /// </summary>
        PARVORDER,

        /// <summary>
        /// Superfamily rank
        /// </summary>
        SUPERFAMILY,

        /// <summary>
        /// Family rank
        /// </summary>
        FAMILY,

        /// <summary>
        /// Subfamily rank
        /// </summary>
        SUBFAMILY,

        /// <summary>
        /// Tribe rank
        /// </summary>
        TRIBE,

        /// <summary>
        /// Subtribe rank
        /// </summary>
        SUBTRIBE,

        /// <summary>
        /// Genus rank
        /// </summary>
        GENUS,

        /// <summary>
        /// Subgenus rank
        /// </summary>
        SUBGENUS,

        /// <summary>
        /// Species group rank
        /// </summary>
        SPECIES_GROUP,

        /// <summary>
        /// Species subgroup rank
        /// </summary>
        SPECIES_SUBGROUP,

        /// <summary>
        /// Species rank
        /// </summary>
        SPECIES,

        /// <summary>
        /// Subspecies rank
        /// </summary>
        SUBSPECIES,

        /// <summary>
        /// Varietas rank
        /// </summary>
        VARIETAS,

        /// <summary>
        /// Forma rank
        /// </summary>
        FORMA
    }

    /// <summary>
    /// Count types for different data categories
    /// </summary>
    public enum CountType
    {
        /// <summary>
        /// Total count
        /// </summary>
        TOTAL,

        /// <summary>
        /// Genome count
        /// </summary>
        GENOME,

        /// <summary>
        /// Assembly count
        /// </summary>
        ASSEMBLY,

        /// <summary>
        /// Gene count
        /// </summary>
        GENE,

        /// <summary>
        /// Protein count
        /// </summary>
        PROTEIN,

        /// <summary>
        /// Sequence count
        /// </summary>
        SEQUENCE,

        /// <summary>
        /// Nucleotide count
        /// </summary>
        NUCLEOTIDE,

        /// <summary>
        /// Publication count
        /// </summary>
        PUBLICATION,

        /// <summary>
        /// BioSample count
        /// </summary>
        BIOSAMPLE,

        /// <summary>
        /// SRA count
        /// </summary>
        SRA
    }
}

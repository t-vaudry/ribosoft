using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Enums;

/// <summary>
/// Annotation types for assembly data
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnotationForAssemblyType
{
    /// <summary>
    /// Default annotation type
    /// </summary>
    DEFAULT,

    /// <summary>
    /// Genome GFF annotation
    /// </summary>
    GENOME_GFF,

    /// <summary>
    /// Genome GBFF annotation
    /// </summary>
    GENOME_GBFF,

    /// <summary>
    /// RNA FASTA sequences
    /// </summary>
    RNA_FASTA,

    /// <summary>
    /// Protein FASTA sequences
    /// </summary>
    PROT_FASTA,

    /// <summary>
    /// Genome GTF annotation
    /// </summary>
    GENOME_GTF,

    /// <summary>
    /// CDS FASTA sequences
    /// </summary>
    CDS_FASTA,

    /// <summary>
    /// Genome FASTA sequences
    /// </summary>
    GENOME_FASTA,

    /// <summary>
    /// Sequence report
    /// </summary>
    SEQUENCE_REPORT
}

/// <summary>
/// General annotation types for datasets
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnotationTypes
{
    /// <summary>
    /// Genome sequences
    /// </summary>
    Genome,

    /// <summary>
    /// RNA sequences
    /// </summary>
    Rna,

    /// <summary>
    /// Protein sequences
    /// </summary>
    Protein,

    /// <summary>
    /// CDS sequences
    /// </summary>
    Cds,

    /// <summary>
    /// GFF annotation files
    /// </summary>
    Gff,

    /// <summary>
    /// GTF annotation files
    /// </summary>
    Gtf,

    /// <summary>
    /// GBFF annotation files
    /// </summary>
    Gbff,

    /// <summary>
    /// Sequence reports
    /// </summary>
    SequenceReport
}

/// <summary>
/// Hydration levels for dataset requests
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HydrationLevel
{
    /// <summary>
    /// Fully hydrated with all data
    /// </summary>
    Fully_Hydrated,

    /// <summary>
    /// Data report only
    /// </summary>
    Data_Report_Only
}

/// <summary>
/// Annotation types for organelle data
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnnotationForOrganelleType
{
    /// <summary>
    /// Default annotation type
    /// </summary>
    DEFAULT,

    /// <summary>
    /// Genome FASTA sequences
    /// </summary>
    GENOME_FASTA,

    /// <summary>
    /// CDS FASTA sequences
    /// </summary>
    CDS_FASTA,

    /// <summary>
    /// Protein FASTA sequences
    /// </summary>
    PROTEIN_FASTA
}

/// <summary>
/// Assembly dataset request resolution types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssemblyDatasetRequestResolution
{
    /// <summary>
    /// Fully hydrated dataset with all files
    /// </summary>
    FULLY_HYDRATED,

    /// <summary>
    /// Data report only, no sequence files
    /// </summary>
    DATA_REPORT_ONLY,

    /// <summary>
    /// Chromosome level resolution
    /// </summary>
    Chromosome
}

/// <summary>
/// Assembly dataset reports request content types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssemblyDatasetReportsRequestContentType
{
    /// <summary>
    /// Complete report
    /// </summary>
    COMPLETE,

    /// <summary>
    /// Complete report (alternative naming)
    /// </summary>
    Complete,

    /// <summary>
    /// Assembly accession only
    /// </summary>
    ASSM_ACC,

    /// <summary>
    /// Paired accession
    /// </summary>
    PAIRED_ACC
}

/// <summary>
/// Assembly link types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssemblyLinkType
{
    /// <summary>
    /// Genome Data Viewer link
    /// </summary>
    GDV_LINK,

    /// <summary>
    /// Statistics link
    /// </summary>
    Stats
}

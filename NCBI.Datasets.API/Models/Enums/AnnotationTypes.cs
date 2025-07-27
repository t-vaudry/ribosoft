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
/// Gene annotation types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneAnnotationType
{
    /// <summary>
    /// Gene FASTA sequences
    /// </summary>
    GENE_FASTA,

    /// <summary>
    /// RNA FASTA sequences
    /// </summary>
    RNA_FASTA,

    /// <summary>
    /// Protein FASTA sequences
    /// </summary>
    PROTEIN_FASTA,

    /// <summary>
    /// CDS FASTA sequences
    /// </summary>
    CDS_FASTA,

    /// <summary>
    /// Gene GFF annotation
    /// </summary>
    GENE_GFF,

    /// <summary>
    /// Gene GTF annotation
    /// </summary>
    GENE_GTF,

    /// <summary>
    /// Gene data report
    /// </summary>
    GENE_DATA_REPORT,

    /// <summary>
    /// Product report
    /// </summary>
    PRODUCT_REPORT
}

/// <summary>
/// Gene types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneType
{
    /// <summary>
    /// Protein coding gene
    /// </summary>
    PROTEIN_CODING,

    /// <summary>
    /// Non-coding RNA gene
    /// </summary>
    NCRNA,

    /// <summary>
    /// Pseudogene
    /// </summary>
    PSEUDOGENE,

    /// <summary>
    /// Other gene types
    /// </summary>
    OTHER,

    /// <summary>
    /// Unknown gene type
    /// </summary>
    UNKNOWN
}

/// <summary>
/// Gene dataset reports request content types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneDatasetReportsRequestContentType
{
    /// <summary>
    /// Complete report
    /// </summary>
    COMPLETE,

    /// <summary>
    /// IDs only
    /// </summary>
    IDS_ONLY,

    /// <summary>
    /// Counts only
    /// </summary>
    COUNTS_ONLY
}

/// <summary>
/// Gene dataset request content types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneDatasetRequestContentType
{
    /// <summary>
    /// Complete dataset
    /// </summary>
    COMPLETE,

    /// <summary>
    /// IDs only
    /// </summary>
    IDS_ONLY
}

/// <summary>
/// Gene link types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneLinkType
{
    /// <summary>
    /// NCBI Gene link
    /// </summary>
    NCBI_GENE,

    /// <summary>
    /// Ensembl link
    /// </summary>
    ENSEMBL,

    /// <summary>
    /// UniProt link
    /// </summary>
    UNIPROT,

    /// <summary>
    /// RefSeq link
    /// </summary>
    REFSEQ,

    /// <summary>
    /// GenBank link
    /// </summary>
    GENBANK
}

/// <summary>
/// Sort field specification
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortField
{
    /// <summary>
    /// Sort by gene ID
    /// </summary>
    GENE_ID,

    /// <summary>
    /// Sort by gene symbol
    /// </summary>
    SYMBOL,

    /// <summary>
    /// Sort by organism
    /// </summary>
    ORGANISM,

    /// <summary>
    /// Sort by gene type
    /// </summary>
    GENE_TYPE,

    /// <summary>
    /// Sort by chromosome
    /// </summary>
    CHROMOSOME,

    /// <summary>
    /// Sort by position
    /// </summary>
    POSITION
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

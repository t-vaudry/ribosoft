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

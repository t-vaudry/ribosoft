using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Virus;

/// <summary>
/// Virus data report page response
/// </summary>
public class VirusDataReportPage
{
    /// <summary>
    /// List of virus reports
    /// </summary>
    [JsonPropertyName("reports")]
    public List<VirusReportMatch> Reports { get; set; } = new();

    /// <summary>
    /// Messages from processing
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ReportMessage> Messages { get; set; } = new();

    /// <summary>
    /// The total count of available virus genomes (ignoring the page_size parameter)
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// A token that can be sent as page_token to retrieve the next page
    /// </summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Individual virus report match
/// </summary>
public class VirusReportMatch
{
    /// <summary>
    /// Virus genome descriptor
    /// </summary>
    [JsonPropertyName("virus")]
    public VirusDescriptor? Virus { get; set; }

    /// <summary>
    /// Query information
    /// </summary>
    [JsonPropertyName("query")]
    public List<string>? Query { get; set; }

    /// <summary>
    /// Warnings
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<string>? Warnings { get; set; }
}

/// <summary>
/// Virus genome descriptor
/// </summary>
public class VirusDescriptor
{
    /// <summary>
    /// Virus genome accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Virus name/title
    /// </summary>
    [JsonPropertyName("virus_name")]
    public string VirusName { get; set; } = string.Empty;

    /// <summary>
    /// Organism information
    /// </summary>
    [JsonPropertyName("organism")]
    public VirusOrganismInfo? Organism { get; set; }

    /// <summary>
    /// Genome length
    /// </summary>
    [JsonPropertyName("length")]
    public int Length { get; set; }

    /// <summary>
    /// Whether the genome is complete
    /// </summary>
    [JsonPropertyName("is_complete")]
    public bool IsComplete { get; set; }

    /// <summary>
    /// Whether the genome is annotated
    /// </summary>
    [JsonPropertyName("is_annotated")]
    public bool IsAnnotated { get; set; }

    /// <summary>
    /// Host organism information
    /// </summary>
    [JsonPropertyName("host")]
    public HostInfo? Host { get; set; }

    /// <summary>
    /// Geographic location
    /// </summary>
    [JsonPropertyName("geo_location")]
    public string? GeoLocation { get; set; }

    /// <summary>
    /// Collection date
    /// </summary>
    [JsonPropertyName("collection_date")]
    public DateTime? CollectionDate { get; set; }

    /// <summary>
    /// Release date
    /// </summary>
    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Update date
    /// </summary>
    [JsonPropertyName("update_date")]
    public DateTime? UpdateDate { get; set; }

    /// <summary>
    /// Source database (RefSeq, GenBank)
    /// </summary>
    [JsonPropertyName("source_database")]
    public string? SourceDatabase { get; set; }

    /// <summary>
    /// Pangolin lineage classification
    /// </summary>
    [JsonPropertyName("pangolin_classification")]
    public string? PangolinClassification { get; set; }

    /// <summary>
    /// BioSample accession
    /// </summary>
    [JsonPropertyName("biosample_accession")]
    public string? BiosampleAccession { get; set; }

    /// <summary>
    /// BioProject accession
    /// </summary>
    [JsonPropertyName("bioproject_accession")]
    public string? BioprojectAccession { get; set; }

    /// <summary>
    /// SRA accessions
    /// </summary>
    [JsonPropertyName("sra_accessions")]
    public List<string>? SraAccessions { get; set; }

    /// <summary>
    /// Virus proteins
    /// </summary>
    [JsonPropertyName("proteins")]
    public List<VirusProtein>? Proteins { get; set; }
}

/// <summary>
/// Virus organism information
/// </summary>
public class VirusOrganismInfo
{
    /// <summary>
    /// NCBI Taxonomy ID
    /// </summary>
    [JsonPropertyName("tax_id")]
    public int TaxId { get; set; }

    /// <summary>
    /// Scientific name
    /// </summary>
    [JsonPropertyName("scientific_name")]
    public string ScientificName { get; set; } = string.Empty;

    /// <summary>
    /// Common name
    /// </summary>
    [JsonPropertyName("common_name")]
    public string? CommonName { get; set; }

    /// <summary>
    /// Taxonomic lineage
    /// </summary>
    [JsonPropertyName("lineage")]
    public List<string>? Lineage { get; set; }
}

/// <summary>
/// Host organism information
/// </summary>
public class HostInfo
{
    /// <summary>
    /// Host taxonomy ID
    /// </summary>
    [JsonPropertyName("tax_id")]
    public int? TaxId { get; set; }

    /// <summary>
    /// Host scientific name
    /// </summary>
    [JsonPropertyName("scientific_name")]
    public string? ScientificName { get; set; }

    /// <summary>
    /// Host common name
    /// </summary>
    [JsonPropertyName("common_name")]
    public string? CommonName { get; set; }
}

/// <summary>
/// Virus protein information
/// </summary>
public class VirusProtein
{
    /// <summary>
    /// Protein accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Protein name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Protein length
    /// </summary>
    [JsonPropertyName("length")]
    public int Length { get; set; }

    /// <summary>
    /// CDS accession
    /// </summary>
    [JsonPropertyName("cds_accession")]
    public string? CdsAccession { get; set; }

    /// <summary>
    /// Gene name
    /// </summary>
    [JsonPropertyName("gene_name")]
    public string? GeneName { get; set; }
}

/// <summary>
/// Virus availability response
/// </summary>
public class VirusAvailabilityReply
{
    /// <summary>
    /// List of valid accessions
    /// </summary>
    [JsonPropertyName("valid_accessions")]
    public List<string> ValidAccessions { get; set; } = new();

    /// <summary>
    /// List of invalid accessions
    /// </summary>
    [JsonPropertyName("invalid_accessions")]
    public List<string> InvalidAccessions { get; set; } = new();

    /// <summary>
    /// Status message
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// SARS-CoV-2 protein dataset summary response
/// </summary>
public class Sars2ProteinDatasetSummary
{
    /// <summary>
    /// List of protein summaries
    /// </summary>
    [JsonPropertyName("proteins")]
    public List<Sars2ProteinSummary> Proteins { get; set; } = new();

    /// <summary>
    /// Total count
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Messages from processing
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ReportMessage> Messages { get; set; } = new();
}

/// <summary>
/// SARS-CoV-2 protein summary
/// </summary>
public class Sars2ProteinSummary
{
    /// <summary>
    /// Protein name
    /// </summary>
    [JsonPropertyName("protein_name")]
    public string ProteinName { get; set; } = string.Empty;

    /// <summary>
    /// Number of sequences
    /// </summary>
    [JsonPropertyName("sequence_count")]
    public int SequenceCount { get; set; }

    /// <summary>
    /// RefSeq sequence count
    /// </summary>
    [JsonPropertyName("refseq_count")]
    public int RefSeqCount { get; set; }

    /// <summary>
    /// GenBank sequence count
    /// </summary>
    [JsonPropertyName("genbank_count")]
    public int GenbankCount { get; set; }

    /// <summary>
    /// Protein sequences
    /// </summary>
    [JsonPropertyName("sequences")]
    public List<VirusProtein>? Sequences { get; set; }
}

/// <summary>
/// Virus annotation report page
/// </summary>
public class VirusAnnotationReportPage
{
    /// <summary>
    /// List of annotation reports
    /// </summary>
    [JsonPropertyName("reports")]
    public List<VirusAnnotationReport> Reports { get; set; } = new();

    /// <summary>
    /// Messages from processing
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ReportMessage> Messages { get; set; } = new();

    /// <summary>
    /// Total count
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Next page token
    /// </summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Individual virus annotation report
/// </summary>
public class VirusAnnotationReport
{
    /// <summary>
    /// Virus genome accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Annotation features
    /// </summary>
    [JsonPropertyName("features")]
    public List<AnnotationFeature> Features { get; set; } = new();

    /// <summary>
    /// Feature counts by type
    /// </summary>
    [JsonPropertyName("feature_counts")]
    public Dictionary<string, int>? FeatureCounts { get; set; }
}

/// <summary>
/// Annotation feature
/// </summary>
public class AnnotationFeature
{
    /// <summary>
    /// Feature type (gene, CDS, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Feature name
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Start position
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>
    /// End position
    /// </summary>
    [JsonPropertyName("end")]
    public int End { get; set; }

    /// <summary>
    /// Strand
    /// </summary>
    [JsonPropertyName("strand")]
    public string? Strand { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    [JsonPropertyName("product")]
    public string? Product { get; set; }
}

/// <summary>
/// Report message
/// </summary>
public class ReportMessage
{
    /// <summary>
    /// Message level (info, warning, error)
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Message text
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional context
    /// </summary>
    [JsonPropertyName("context")]
    public Dictionary<string, object>? Context { get; set; }
}

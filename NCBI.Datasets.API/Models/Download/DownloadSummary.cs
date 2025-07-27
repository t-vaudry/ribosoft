using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Download;

/// <summary>
/// Download summary information
/// </summary>
public class DownloadSummary
{
    /// <summary>
    /// The number of records for the requested filter
    /// </summary>
    [JsonPropertyName("record_count")]
    public int RecordCount { get; set; }

    /// <summary>
    /// For backwards compatibility with old VirusDatasetSummary
    /// </summary>
    [JsonPropertyName("assembly_count")]
    public int AssemblyCount { get; set; }

    /// <summary>
    /// The latest date on which the resource was updated
    /// </summary>
    [JsonPropertyName("resource_updated_on")]
    public DateTime? ResourceUpdatedOn { get; set; }

    /// <summary>
    /// Hydrated download information
    /// </summary>
    [JsonPropertyName("hydrated")]
    public DownloadSummaryHydrated? Hydrated { get; set; }

    /// <summary>
    /// Dehydrated download information
    /// </summary>
    [JsonPropertyName("dehydrated")]
    public DownloadSummaryDehydrated? Dehydrated { get; set; }

    /// <summary>
    /// Errors encountered during processing
    /// </summary>
    [JsonPropertyName("errors")]
    public List<ReportError>? Errors { get; set; }

    /// <summary>
    /// Messages from processing
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ReportMessage>? Messages { get; set; }

    /// <summary>
    /// Available files in the download
    /// </summary>
    [JsonPropertyName("available_files")]
    public DownloadSummaryAvailableFiles? AvailableFiles { get; set; }
}

/// <summary>
/// Hydrated download information
/// </summary>
public class DownloadSummaryHydrated
{
    /// <summary>
    /// Estimated file size in megabytes
    /// </summary>
    [JsonPropertyName("estimated_file_size_mb")]
    public int EstimatedFileSizeMb { get; set; }

    /// <summary>
    /// Download URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// CLI download command line
    /// </summary>
    [JsonPropertyName("cli_download_command_line")]
    public string CliDownloadCommandLine { get; set; } = string.Empty;
}

/// <summary>
/// Dehydrated download information
/// </summary>
public class DownloadSummaryDehydrated
{
    /// <summary>
    /// Estimated file size in megabytes
    /// </summary>
    [JsonPropertyName("estimated_file_size_mb")]
    public int EstimatedFileSizeMb { get; set; }

    /// <summary>
    /// Download URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// CLI download command line
    /// </summary>
    [JsonPropertyName("cli_download_command_line")]
    public string CliDownloadCommandLine { get; set; } = string.Empty;

    /// <summary>
    /// CLI rehydrate command line
    /// </summary>
    [JsonPropertyName("cli_rehydrate_command_line")]
    public string CliRehydrateCommandLine { get; set; } = string.Empty;
}

/// <summary>
/// Available files in the download
/// </summary>
public class DownloadSummaryAvailableFiles
{
    /// <summary>
    /// All genomic FASTA files
    /// </summary>
    [JsonPropertyName("all_genomic_fasta")]
    public DownloadSummaryFileSummary? AllGenomicFasta { get; set; }

    /// <summary>
    /// Genome GFF files
    /// </summary>
    [JsonPropertyName("genome_gff")]
    public DownloadSummaryFileSummary? GenomeGff { get; set; }

    /// <summary>
    /// Genome GBFF files
    /// </summary>
    [JsonPropertyName("genome_gbff")]
    public DownloadSummaryFileSummary? GenomeGbff { get; set; }

    /// <summary>
    /// RNA FASTA files
    /// </summary>
    [JsonPropertyName("rna_fasta")]
    public DownloadSummaryFileSummary? RnaFasta { get; set; }

    /// <summary>
    /// Protein FASTA files
    /// </summary>
    [JsonPropertyName("prot_fasta")]
    public DownloadSummaryFileSummary? ProtFasta { get; set; }

    /// <summary>
    /// Genome GTF files
    /// </summary>
    [JsonPropertyName("genome_gtf")]
    public DownloadSummaryFileSummary? GenomeGtf { get; set; }

    /// <summary>
    /// CDS FASTA files
    /// </summary>
    [JsonPropertyName("cds_fasta")]
    public DownloadSummaryFileSummary? CdsFasta { get; set; }

    /// <summary>
    /// Sequence report files
    /// </summary>
    [JsonPropertyName("sequence_report")]
    public DownloadSummaryFileSummary? SequenceReport { get; set; }

    /// <summary>
    /// Annotation report files
    /// </summary>
    [JsonPropertyName("annotation_report")]
    public DownloadSummaryFileSummary? AnnotationReport { get; set; }
}

/// <summary>
/// File summary information
/// </summary>
public class DownloadSummaryFileSummary
{
    /// <summary>
    /// Number of files
    /// </summary>
    [JsonPropertyName("file_count")]
    public int FileCount { get; set; }

    /// <summary>
    /// Size in megabytes
    /// </summary>
    [JsonPropertyName("size_mb")]
    public float SizeMb { get; set; }
}

/// <summary>
/// Report error information
/// </summary>
public class ReportError
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Report message information
/// </summary>
public class ReportMessage
{
    /// <summary>
    /// Message level
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Message text
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

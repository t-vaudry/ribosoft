using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Genome;

/// <summary>
/// Assembly dataset availability response
/// </summary>
public class AssemblyDatasetAvailability
{
    /// <summary>
    /// List of valid assembly accessions
    /// </summary>
    [JsonPropertyName("valid_assemblies")]
    public List<string> ValidAssemblies { get; set; } = new();

    /// <summary>
    /// List of invalid assembly accessions
    /// </summary>
    [JsonPropertyName("invalid_assemblies")]
    public List<string> InvalidAssemblies { get; set; } = new();
}

/// <summary>
/// Assembly links response
/// </summary>
public class AssemblyLinksReply
{
    /// <summary>
    /// Assembly links
    /// </summary>
    [JsonPropertyName("assembly_links")]
    public List<AssemblyLink> AssemblyLinks { get; set; } = new();
}

/// <summary>
/// Individual assembly link
/// </summary>
public class AssemblyLink
{
    /// <summary>
    /// Assembly accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Link URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Link type
    /// </summary>
    [JsonPropertyName("link_type")]
    public string LinkType { get; set; } = string.Empty;
}

/// <summary>
/// Assembly dataset report response
/// </summary>
public class AssemblyDatasetReport
{
    /// <summary>
    /// Total count of assemblies
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Page token for pagination
    /// </summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }

    /// <summary>
    /// List of assembly reports
    /// </summary>
    [JsonPropertyName("reports")]
    public List<AssemblyReport> Reports { get; set; } = new();
}

/// <summary>
/// Individual assembly report
/// </summary>
public class AssemblyReport
{
    /// <summary>
    /// Assembly accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Assembly name
    /// </summary>
    [JsonPropertyName("assembly_name")]
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Organism name
    /// </summary>
    [JsonPropertyName("organism_name")]
    public string OrganismName { get; set; } = string.Empty;

    /// <summary>
    /// Taxon ID
    /// </summary>
    [JsonPropertyName("taxid")]
    public int Taxid { get; set; }

    /// <summary>
    /// Assembly level
    /// </summary>
    [JsonPropertyName("assembly_level")]
    public string AssemblyLevel { get; set; } = string.Empty;

    /// <summary>
    /// Assembly status
    /// </summary>
    [JsonPropertyName("assembly_status")]
    public string AssemblyStatus { get; set; } = string.Empty;

    /// <summary>
    /// Release date
    /// </summary>
    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Submission date
    /// </summary>
    [JsonPropertyName("submission_date")]
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// BioProject accession
    /// </summary>
    [JsonPropertyName("bioproject_accession")]
    public string? BioprojectAccession { get; set; }

    /// <summary>
    /// BioSample accession
    /// </summary>
    [JsonPropertyName("biosample_accession")]
    public string? BiosampleAccession { get; set; }

    /// <summary>
    /// Assembly statistics
    /// </summary>
    [JsonPropertyName("assembly_stats")]
    public AssemblyStats? AssemblyStats { get; set; }
}

/// <summary>
/// Assembly statistics
/// </summary>
public class AssemblyStats
{
    /// <summary>
    /// Total sequence length
    /// </summary>
    [JsonPropertyName("total_sequence_length")]
    public long TotalSequenceLength { get; set; }

    /// <summary>
    /// Total ungapped length
    /// </summary>
    [JsonPropertyName("total_ungapped_length")]
    public long TotalUngappedLength { get; set; }

    /// <summary>
    /// Number of contigs
    /// </summary>
    [JsonPropertyName("number_of_contigs")]
    public int NumberOfContigs { get; set; }

    /// <summary>
    /// Number of scaffolds
    /// </summary>
    [JsonPropertyName("number_of_scaffolds")]
    public int NumberOfScaffolds { get; set; }

    /// <summary>
    /// Contig N50
    /// </summary>
    [JsonPropertyName("contig_n50")]
    public long ContigN50 { get; set; }

    /// <summary>
    /// Scaffold N50
    /// </summary>
    [JsonPropertyName("scaffold_n50")]
    public long ScaffoldN50 { get; set; }

    /// <summary>
    /// GC percent
    /// </summary>
    [JsonPropertyName("gc_percent")]
    public float GcPercent { get; set; }
}

/// <summary>
/// CheckM histogram response
/// </summary>
public class AssemblyCheckMHistogramReply
{
    /// <summary>
    /// Species taxon ID
    /// </summary>
    [JsonPropertyName("species_taxid")]
    public int SpeciesTaxid { get; set; }

    /// <summary>
    /// Histogram intervals
    /// </summary>
    [JsonPropertyName("histogram_intervals")]
    public List<AssemblyCheckMHistogramInterval> HistogramIntervals { get; set; } = new();
}

/// <summary>
/// CheckM histogram interval
/// </summary>
public class AssemblyCheckMHistogramInterval
{
    /// <summary>
    /// Starting position for this interval
    /// </summary>
    [JsonPropertyName("start_pos")]
    public float StartPos { get; set; }

    /// <summary>
    /// Ending position for this interval
    /// </summary>
    [JsonPropertyName("stop_pos")]
    public float StopPos { get; set; }

    /// <summary>
    /// Number of elements in this interval
    /// </summary>
    [JsonPropertyName("count")]
    public float Count { get; set; }
}

/// <summary>
/// Sequence assemblies response
/// </summary>
public class SequenceAssembliesReply
{
    /// <summary>
    /// List of sequence assemblies
    /// </summary>
    [JsonPropertyName("sequence_assemblies")]
    public List<SequenceAssembly> SequenceAssemblies { get; set; } = new();
}

/// <summary>
/// Individual sequence assembly
/// </summary>
public class SequenceAssembly
{
    /// <summary>
    /// Sequence accession
    /// </summary>
    [JsonPropertyName("sequence_accession")]
    public string SequenceAccession { get; set; } = string.Empty;

    /// <summary>
    /// Assembly accession
    /// </summary>
    [JsonPropertyName("assembly_accession")]
    public string AssemblyAccession { get; set; } = string.Empty;

    /// <summary>
    /// Sequence length
    /// </summary>
    [JsonPropertyName("sequence_length")]
    public long SequenceLength { get; set; }

    /// <summary>
    /// Sequence role
    /// </summary>
    [JsonPropertyName("sequence_role")]
    public string SequenceRole { get; set; } = string.Empty;
}

/// <summary>
/// Sequence reports response
/// </summary>
public class SequenceReportsReply
{
    /// <summary>
    /// List of sequence reports
    /// </summary>
    [JsonPropertyName("sequence_reports")]
    public List<SequenceReport> SequenceReports { get; set; } = new();
}

/// <summary>
/// Individual sequence report
/// </summary>
public class SequenceReport
{
    /// <summary>
    /// Sequence accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Sequence length
    /// </summary>
    [JsonPropertyName("length")]
    public long Length { get; set; }

    /// <summary>
    /// Sequence name
    /// </summary>
    [JsonPropertyName("sequence_name")]
    public string SequenceName { get; set; } = string.Empty;

    /// <summary>
    /// Chromosome name
    /// </summary>
    [JsonPropertyName("chr_name")]
    public string ChromosomeName { get; set; } = string.Empty;

    /// <summary>
    /// Assembly unit
    /// </summary>
    [JsonPropertyName("assembly_unit")]
    public string AssemblyUnit { get; set; } = string.Empty;
}

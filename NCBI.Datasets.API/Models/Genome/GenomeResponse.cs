using System.Text.Json.Serialization;
using NCBI.Datasets.API.Converters;

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
    [JsonConverter(typeof(FlexibleIntConverter))]
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
    /// Current accession
    /// </summary>
    [JsonPropertyName("current_accession")]
    public string CurrentAccession { get; set; } = string.Empty;

    /// <summary>
    /// Paired accession
    /// </summary>
    [JsonPropertyName("paired_accession")]
    public string PairedAccession { get; set; } = string.Empty;

    /// <summary>
    /// Source database
    /// </summary>
    [JsonPropertyName("source_database")]
    public string SourceDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Organism information
    /// </summary>
    [JsonPropertyName("organism")]
    public OrganismInfo? Organism { get; set; }

    /// <summary>
    /// Assembly information
    /// </summary>
    [JsonPropertyName("assembly_info")]
    public AssemblyInfo? AssemblyInfo { get; set; }

    /// <summary>
    /// Assembly statistics
    /// </summary>
    [JsonPropertyName("assembly_stats")]
    public AssemblyStats? AssemblyStats { get; set; }

    // Legacy properties for backward compatibility
    /// <summary>
    /// Assembly name (legacy - use AssemblyInfo.AssemblyName)
    /// </summary>
    [JsonIgnore]
    public string AssemblyName => AssemblyInfo?.AssemblyName ?? string.Empty;

    /// <summary>
    /// Organism name (legacy - use Organism.OrganismName)
    /// </summary>
    [JsonIgnore]
    public string OrganismName => Organism?.OrganismName ?? string.Empty;

    /// <summary>
    /// Taxon ID (legacy - use Organism.TaxId)
    /// </summary>
    [JsonIgnore]
    public int Taxid => Organism?.TaxId ?? 0;

    /// <summary>
    /// Assembly level (legacy - use AssemblyInfo.AssemblyLevel)
    /// </summary>
    [JsonIgnore]
    public string AssemblyLevel => AssemblyInfo?.AssemblyLevel ?? string.Empty;

    /// <summary>
    /// Assembly status (legacy - use AssemblyInfo.AssemblyStatus)
    /// </summary>
    [JsonIgnore]
    public string AssemblyStatus => AssemblyInfo?.AssemblyStatus ?? string.Empty;

    /// <summary>
    /// Release date (legacy - use AssemblyInfo.ReleaseDate)
    /// </summary>
    [JsonIgnore]
    public DateTime? ReleaseDate => AssemblyInfo?.ReleaseDate;

    /// <summary>
    /// Submission date (legacy - not available in new structure)
    /// </summary>
    [JsonIgnore]
    public DateTime? SubmissionDate => null;

    /// <summary>
    /// BioProject accession (legacy - use AssemblyInfo.BioprojectAccession)
    /// </summary>
    [JsonIgnore]
    public string? BioprojectAccession => AssemblyInfo?.BioprojectAccession;

    /// <summary>
    /// BioSample accession (legacy - not available in new structure)
    /// </summary>
    [JsonIgnore]
    public string? BiosampleAccession => null;
}

/// <summary>
/// Organism information
/// </summary>
public class OrganismInfo
{
    /// <summary>
    /// Taxonomy ID
    /// </summary>
    [JsonPropertyName("tax_id")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int TaxId { get; set; }

    /// <summary>
    /// Organism name
    /// </summary>
    [JsonPropertyName("organism_name")]
    public string OrganismName { get; set; } = string.Empty;

    /// <summary>
    /// Common name
    /// </summary>
    [JsonPropertyName("common_name")]
    public string CommonName { get; set; } = string.Empty;
}

/// <summary>
/// Assembly information
/// </summary>
public class AssemblyInfo
{
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
    /// Assembly name
    /// </summary>
    [JsonPropertyName("assembly_name")]
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Assembly type
    /// </summary>
    [JsonPropertyName("assembly_type")]
    public string AssemblyType { get; set; } = string.Empty;

    /// <summary>
    /// BioProject accession
    /// </summary>
    [JsonPropertyName("bioproject_accession")]
    public string BioprojectAccession { get; set; } = string.Empty;

    /// <summary>
    /// Release date
    /// </summary>
    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Submitter
    /// </summary>
    [JsonPropertyName("submitter")]
    public string Submitter { get; set; } = string.Empty;

    /// <summary>
    /// RefSeq category
    /// </summary>
    [JsonPropertyName("refseq_category")]
    public string RefseqCategory { get; set; } = string.Empty;

    /// <summary>
    /// Synonym
    /// </summary>
    [JsonPropertyName("synonym")]
    public string Synonym { get; set; } = string.Empty;
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
    [JsonConverter(typeof(FlexibleLongConverter))]
    public long TotalSequenceLength { get; set; }

    /// <summary>
    /// Total ungapped length
    /// </summary>
    [JsonPropertyName("total_ungapped_length")]
    [JsonConverter(typeof(FlexibleLongConverter))]
    public long TotalUngappedLength { get; set; }

    /// <summary>
    /// Number of contigs
    /// </summary>
    [JsonPropertyName("number_of_contigs")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int NumberOfContigs { get; set; }

    /// <summary>
    /// Number of scaffolds
    /// </summary>
    [JsonPropertyName("number_of_scaffolds")]
    [JsonConverter(typeof(FlexibleIntConverter))]
    public int NumberOfScaffolds { get; set; }

    /// <summary>
    /// Contig N50
    /// </summary>
    [JsonPropertyName("contig_n50")]
    [JsonConverter(typeof(FlexibleLongConverter))]
    public long ContigN50 { get; set; }

    /// <summary>
    /// Scaffold N50
    /// </summary>
    [JsonPropertyName("scaffold_n50")]
    [JsonConverter(typeof(FlexibleLongConverter))]
    public long ScaffoldN50 { get; set; }

    /// <summary>
    /// GC percent
    /// </summary>
    [JsonPropertyName("gc_percent")]
    [JsonConverter(typeof(FlexibleFloatConverter))]
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
    [JsonConverter(typeof(FlexibleIntConverter))]
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
    [JsonConverter(typeof(FlexibleFloatConverter))]
    public float StartPos { get; set; }

    /// <summary>
    /// Ending position for this interval
    /// </summary>
    [JsonPropertyName("stop_pos")]
    [JsonConverter(typeof(FlexibleFloatConverter))]
    public float StopPos { get; set; }

    /// <summary>
    /// Number of elements in this interval
    /// </summary>
    [JsonPropertyName("count")]
    [JsonConverter(typeof(FlexibleFloatConverter))]
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
    [JsonConverter(typeof(FlexibleLongConverter))]
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
    [JsonConverter(typeof(FlexibleLongConverter))]
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

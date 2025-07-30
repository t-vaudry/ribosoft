using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Gene;

/// <summary>
/// Gene data report page response
/// </summary>
public class GeneDataReportPage
{
    /// <summary>
    /// List of gene reports
    /// </summary>
    [JsonPropertyName("reports")]
    public List<GeneReportMatch> Reports { get; set; } = new();

    /// <summary>
    /// Messages from processing
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ReportMessage> Messages { get; set; } = new();

    /// <summary>
    /// The total count of available genes (ignoring the page_size parameter)
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
/// Individual gene report match
/// </summary>
public class GeneReportMatch
{
    /// <summary>
    /// Gene descriptor
    /// </summary>
    [JsonPropertyName("gene")]
    public GeneDescriptor? Gene { get; set; }

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
/// Gene descriptor
/// </summary>
public class GeneDescriptor
{
    /// <summary>
    /// NCBI Gene ID
    /// </summary>
    [JsonPropertyName("gene_id")]
    public int GeneId { get; set; }

    /// <summary>
    /// Gene symbol
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Gene description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Organism information
    /// </summary>
    [JsonPropertyName("organism")]
    public OrganismInfo? Organism { get; set; }

    /// <summary>
    /// Gene type
    /// </summary>
    [JsonPropertyName("gene_type")]
    public string GeneType { get; set; } = string.Empty;

    /// <summary>
    /// Chromosomal location
    /// </summary>
    [JsonPropertyName("location")]
    public GeneLocation? Location { get; set; }

    /// <summary>
    /// Alternative symbols
    /// </summary>
    [JsonPropertyName("synonyms")]
    public List<string>? Synonyms { get; set; }

    /// <summary>
    /// Cross-references
    /// </summary>
    [JsonPropertyName("cross_references")]
    public List<CrossReference>? CrossReferences { get; set; }

    /// <summary>
    /// Gene products (transcripts/proteins)
    /// </summary>
    [JsonPropertyName("products")]
    public List<GeneProduct>? Products { get; set; }
}

/// <summary>
/// Organism information
/// </summary>
public class OrganismInfo
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
}

/// <summary>
/// Gene chromosomal location
/// </summary>
public class GeneLocation
{
    /// <summary>
    /// Chromosome name
    /// </summary>
    [JsonPropertyName("chromosome")]
    public string Chromosome { get; set; } = string.Empty;

    /// <summary>
    /// Start position
    /// </summary>
    [JsonPropertyName("start")]
    public long Start { get; set; }

    /// <summary>
    /// End position
    /// </summary>
    [JsonPropertyName("end")]
    public long End { get; set; }

    /// <summary>
    /// Strand orientation
    /// </summary>
    [JsonPropertyName("strand")]
    public string Strand { get; set; } = string.Empty;

    /// <summary>
    /// Assembly accession
    /// </summary>
    [JsonPropertyName("assembly_accession")]
    public string? AssemblyAccession { get; set; }
}

/// <summary>
/// Cross-reference to external databases
/// </summary>
public class CrossReference
{
    /// <summary>
    /// Database name
    /// </summary>
    [JsonPropertyName("database")]
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Identifier in the external database
    /// </summary>
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// URL to the external resource
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Gene product (transcript or protein)
/// </summary>
public class GeneProduct
{
    /// <summary>
    /// Product accession
    /// </summary>
    [JsonPropertyName("accession")]
    public string Accession { get; set; } = string.Empty;

    /// <summary>
    /// Product type (mRNA, protein, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Product name/description
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Length of the product
    /// </summary>
    [JsonPropertyName("length")]
    public int? Length { get; set; }
}

/// <summary>
/// Gene counts by taxon response
/// </summary>
public class GeneCountsByTaxonReply
{
    /// <summary>
    /// Taxon ID
    /// </summary>
    [JsonPropertyName("taxon")]
    public string Taxon { get; set; } = string.Empty;

    /// <summary>
    /// Gene counts by type
    /// </summary>
    [JsonPropertyName("counts")]
    public List<GeneTypeAndCount> Counts { get; set; } = new();

    /// <summary>
    /// Total gene counts
    /// </summary>
    [JsonPropertyName("gene_counts")]
    public GeneCounts? GeneCounts { get; set; }
}

/// <summary>
/// Gene type and count
/// </summary>
public class GeneTypeAndCount
{
    /// <summary>
    /// Gene type
    /// </summary>
    [JsonPropertyName("gene_type")]
    public string GeneType { get; set; } = string.Empty;

    /// <summary>
    /// Count of genes of this type
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// Gene counts summary
/// </summary>
public class GeneCounts
{
    /// <summary>
    /// Total number of annotated genes
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// Count of annotated genes that encode a protein
    /// </summary>
    [JsonPropertyName("protein_coding")]
    public int ProteinCoding { get; set; }

    /// <summary>
    /// Count of transcribed non-coding genes (e.g. lncRNAs, miRNAs, rRNAs, etc.) excludes transcribed pseudogenes
    /// </summary>
    [JsonPropertyName("non_coding")]
    public int NonCoding { get; set; }

    /// <summary>
    /// Count of transcribed and non-transcribed pseudogenes
    /// </summary>
    [JsonPropertyName("pseudogene")]
    public int Pseudogene { get; set; }

    /// <summary>
    /// Count of genic region GeneIDs and non-genic regulatory GeneIDs
    /// </summary>
    [JsonPropertyName("other")]
    public int Other { get; set; }
}

/// <summary>
/// Gene links response
/// </summary>
public class GeneLinksReply
{
    /// <summary>
    /// List of gene links
    /// </summary>
    [JsonPropertyName("links")]
    public List<GeneLink> Links { get; set; } = new();
}

/// <summary>
/// Individual gene link
/// </summary>
public class GeneLink
{
    /// <summary>
    /// Gene ID
    /// </summary>
    [JsonPropertyName("gene_id")]
    public int GeneId { get; set; }

    /// <summary>
    /// Link type
    /// </summary>
    [JsonPropertyName("link_type")]
    public string LinkType { get; set; } = string.Empty;

    /// <summary>
    /// URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Link description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Chromosome summary response
/// </summary>
public class GeneChromosomeSummaryReply
{
    /// <summary>
    /// Taxon ID
    /// </summary>
    [JsonPropertyName("taxon")]
    public string Taxon { get; set; } = string.Empty;

    /// <summary>
    /// Annotation name
    /// </summary>
    [JsonPropertyName("annotation_name")]
    public string AnnotationName { get; set; } = string.Empty;

    /// <summary>
    /// Chromosome summaries
    /// </summary>
    [JsonPropertyName("chromosomes")]
    public List<ChromosomeSummary> Chromosomes { get; set; } = new();
}

/// <summary>
/// Individual chromosome summary
/// </summary>
public class ChromosomeSummary
{
    /// <summary>
    /// Chromosome name
    /// </summary>
    [JsonPropertyName("chromosome")]
    public string Chromosome { get; set; } = string.Empty;

    /// <summary>
    /// Gene counts on this chromosome
    /// </summary>
    [JsonPropertyName("gene_counts")]
    public GeneCounts? GeneCounts { get; set; }

    /// <summary>
    /// Chromosome length
    /// </summary>
    [JsonPropertyName("length")]
    public long? Length { get; set; }
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

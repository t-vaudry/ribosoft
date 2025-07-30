using NCBI.Datasets.API.Models.Enums;
using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Models.Gene;

/// <summary>
/// Request for gene dataset reports
/// </summary>
public class GeneDatasetReportsRequest
{
    /// <summary>
    /// Return either gene-ids, or entire gene metadata
    /// </summary>
    [JsonPropertyName("returned_content")]
    public GeneDatasetReportsRequestContentType? ReturnedContent { get; set; }

    /// <summary>
    /// NCBI gene IDs
    /// </summary>
    [JsonPropertyName("gene_ids")]
    public List<int>? GeneIds { get; set; }

    /// <summary>
    /// RNA or Protein accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string>? Accessions { get; set; }

    /// <summary>
    /// Gene symbols for a specific taxon
    /// </summary>
    [JsonPropertyName("symbols_for_taxon")]
    public GeneDatasetReportsRequestSymbolsForTaxon? SymbolsForTaxon { get; set; }

    /// <summary>
    /// NCBI Taxonomy ID or name (common or scientific) that the genes are annotated at
    /// </summary>
    [JsonPropertyName("taxon")]
    public string? Taxon { get; set; }

    /// <summary>
    /// Gene locus tags
    /// </summary>
    [JsonPropertyName("locus_tags")]
    public List<string>? LocusTags { get; set; }

    /// <summary>
    /// Specify which fields to include in the tabular report
    /// </summary>
    [JsonPropertyName("table_fields")]
    public List<string>? TableFields { get; set; }

    /// <summary>
    /// Optional pre-defined template for processing a tabular data request
    /// </summary>
    [JsonPropertyName("table_format")]
    public string? TableFormat { get; set; }

    /// <summary>
    /// Include tabular header in the response
    /// </summary>
    [JsonPropertyName("include_tabular_header")]
    public bool? IncludeTabularHeader { get; set; }

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [JsonPropertyName("page_size")]
    public int? PageSize { get; set; }

    /// <summary>
    /// Page token for pagination
    /// </summary>
    [JsonPropertyName("page_token")]
    public string? PageToken { get; set; }

    /// <summary>
    /// Gene types to filter by
    /// </summary>
    [JsonPropertyName("types")]
    public List<GeneType>? Types { get; set; }

    /// <summary>
    /// Accession filter
    /// </summary>
    [JsonPropertyName("accession_filter")]
    public List<string>? AccessionFilter { get; set; }

    /// <summary>
    /// For queries including a tax-id, include any matching genes annotated on taxa below the selected taxon
    /// </summary>
    [JsonPropertyName("tax_search_subtree")]
    public bool? TaxSearchSubtree { get; set; }

    /// <summary>
    /// Sort fields
    /// </summary>
    [JsonPropertyName("sort")]
    public List<SortField>? Sort { get; set; }
}

/// <summary>
/// Gene symbols for a specific taxon
/// </summary>
public class GeneDatasetReportsRequestSymbolsForTaxon
{
    /// <summary>
    /// Gene symbols
    /// </summary>
    [JsonPropertyName("symbols")]
    public List<string> Symbols { get; set; } = new();

    /// <summary>
    /// Taxon for provided gene symbol
    /// </summary>
    [JsonPropertyName("taxon")]
    public string Taxon { get; set; } = string.Empty;
}

/// <summary>
/// Request for gene dataset
/// </summary>
public class GeneDatasetRequest
{
    /// <summary>
    /// NCBI gene IDs
    /// </summary>
    [JsonPropertyName("gene_ids")]
    public List<int>? GeneIds { get; set; }

    /// <summary>
    /// RNA or Protein accessions
    /// </summary>
    [JsonPropertyName("accessions")]
    public List<string>? Accessions { get; set; }

    /// <summary>
    /// Gene symbols for a specific taxon
    /// </summary>
    [JsonPropertyName("symbols_for_taxon")]
    public GeneDatasetReportsRequestSymbolsForTaxon? SymbolsForTaxon { get; set; }

    /// <summary>
    /// NCBI Taxonomy ID or name (common or scientific) that the genes are annotated at
    /// </summary>
    [JsonPropertyName("taxon")]
    public string? Taxon { get; set; }

    /// <summary>
    /// Gene locus tags
    /// </summary>
    [JsonPropertyName("locus_tags")]
    public List<string>? LocusTags { get; set; }

    /// <summary>
    /// Return either gene-ids, or entire gene metadata
    /// </summary>
    [JsonPropertyName("returned_content")]
    public GeneDatasetRequestContentType? ReturnedContent { get; set; }

    /// <summary>
    /// Include annotation types
    /// </summary>
    [JsonPropertyName("include_annotation_type")]
    public List<GeneAnnotationType>? IncludeAnnotationType { get; set; }

    /// <summary>
    /// Output filename
    /// </summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}

/// <summary>
/// Request for gene counts by taxon
/// </summary>
public class GeneCountsByTaxonRequest
{
    /// <summary>
    /// Taxon for provided gene symbol
    /// </summary>
    [JsonPropertyName("taxon")]
    public string Taxon { get; set; } = string.Empty;
}

/// <summary>
/// Request for gene orthologs
/// </summary>
public class OrthologRequest
{
    /// <summary>
    /// Gene ID for ortholog search
    /// </summary>
    [JsonPropertyName("gene_id")]
    public int GeneId { get; set; }

    /// <summary>
    /// Taxon filters for orthologs
    /// </summary>
    [JsonPropertyName("taxon_filter")]
    public List<string>? TaxonFilter { get; set; }

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [JsonPropertyName("page_size")]
    public int? PageSize { get; set; }

    /// <summary>
    /// Page token for pagination
    /// </summary>
    [JsonPropertyName("page_token")]
    public string? PageToken { get; set; }
}

/// <summary>
/// Request for gene links
/// </summary>
public class GeneLinksRequest
{
    /// <summary>
    /// NCBI gene IDs
    /// </summary>
    [JsonPropertyName("gene_ids")]
    public List<int> GeneIds { get; set; } = new();

    /// <summary>
    /// Link types to retrieve
    /// </summary>
    [JsonPropertyName("link_types")]
    public List<GeneLinkType>? LinkTypes { get; set; }
}

/// <summary>
/// Request for chromosome summary
/// </summary>
public class GeneChromosomeSummaryRequest
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
    /// Chromosome names to include
    /// </summary>
    [JsonPropertyName("chromosomes")]
    public List<string>? Chromosomes { get; set; }
}

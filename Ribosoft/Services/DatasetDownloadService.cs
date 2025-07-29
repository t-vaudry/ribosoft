using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Hangfire;
using NCBI.Datasets.API;
using NCBI.Datasets.API.Models.Genome;
using NCBI.Datasets.API.Services;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Models.ViewModels;
using Ribosoft.Jobs;

namespace Ribosoft.Services
{
    /*! \interface IDatasetDownloadService
     * \brief Interface for dataset download service
     */
    public interface IDatasetDownloadService
    {
        Task<List<AvailableDatasetViewModel>> GetAvailableDatasetsAsync(string? searchTerm = null, int limit = 100);
        Task<string> RequestDownloadAsync(DatasetDownloadRequestViewModel request, string requestedBy);
        Task<List<DatasetDownload>> GetDownloadHistoryAsync();
        Task<DatasetDownload?> GetDownloadStatusAsync(int downloadId);
        Task CancelDownloadAsync(int downloadId);
        Task RetryDownloadAsync(int downloadId);
    }

    /*! \class DatasetDownloadService
     * \brief Service for managing NCBI dataset downloads
     */
    public class DatasetDownloadService : IDatasetDownloadService
    {
        private readonly ApplicationDbContext _context;
        private readonly INCBIDatasetsClient _ncbiClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatasetDownloadService> _logger;

        /*! \fn DatasetDownloadService
         * \brief Constructor
         */
        public DatasetDownloadService(
            ApplicationDbContext context,
            INCBIDatasetsClient ncbiClient,
            IConfiguration configuration,
            ILogger<DatasetDownloadService> logger)
        {
            _context = context;
            _ncbiClient = ncbiClient;
            _configuration = configuration;
            _logger = logger;
        }

        /*! \fn GetAvailableDatasetsAsync
         * \brief Get available datasets from NCBI
         * \param searchTerm Optional search term to filter results
         * \param limit Maximum number of results to return
         * \return List of available datasets
         */
        public async Task<List<AvailableDatasetViewModel>> GetAvailableDatasetsAsync(string? searchTerm = null, int limit = 100)
        {
            try
            {
                _logger.LogInformation("Fetching available datasets from NCBI. Search term: {SearchTerm}, Limit: {Limit}", searchTerm, limit);

                // Get existing downloads to mark already downloaded datasets
                var existingDownloads = await _context.DatasetDownloads
                    .Where(d => d.Status == DatasetDownloadStatus.Completed || d.Status == DatasetDownloadStatus.ReadyForBlast)
                    .Select(d => d.AccessionId)
                    .ToListAsync();

                var currentDownloads = await _context.DatasetDownloads
                    .Where(d => d.Status == DatasetDownloadStatus.Queued || 
                               d.Status == DatasetDownloadStatus.Downloading || 
                               d.Status == DatasetDownloadStatus.Processing)
                    .Select(d => d.AccessionId)
                    .ToListAsync();

                var datasets = new List<AvailableDatasetViewModel>();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Use NCBI API to search for organisms
                    _logger.LogDebug("Searching NCBI taxonomy for: {SearchTerm}", searchTerm);
                    
                    var taxonomyResponse = await _ncbiClient.GetTaxonomySuggestionsAsync(searchTerm, 20);
                    if (!taxonomyResponse.IsSuccess)
                    {
                        _logger.LogWarning("Failed to get taxonomy suggestions for: {SearchTerm}. Error: {Error}", 
                            searchTerm, taxonomyResponse.ErrorMessage);
                        return datasets; // Return empty list if taxonomy search fails
                    }

                    if (taxonomyResponse.Data?.Suggestions == null || !taxonomyResponse.Data.Suggestions.Any())
                    {
                        _logger.LogInformation("No taxonomy suggestions found for: {SearchTerm}", searchTerm);
                        return datasets; // Return empty list if no suggestions
                    }

                    // Get taxonomy IDs from suggestions
                    var taxonIds = taxonomyResponse.Data.Suggestions
                        .Where(s => s.TaxId > 0)
                        .Select(s => s.TaxId)
                        .Take(10) // Limit to first 10 taxonomy matches
                        .ToList();

                    if (taxonIds.Any())
                    {
                        _logger.LogDebug("Found {Count} taxonomy matches, getting assembly reports", taxonIds.Count);
                        
                        // Get assembly reports for these taxonomy IDs (just to get accessions)
                        var assemblyResponse = await _ncbiClient.GetAssemblyDatasetReportsByTaxonAsync(
                            taxonIds, 
                            limit, 
                            NCBI.Datasets.API.Models.Enums.AssemblyDatasetReportsRequestContentType.COMPLETE);
                        
                        if (assemblyResponse.IsSuccess && assemblyResponse.Data?.Reports != null)
                        {
                            // Get accessions from the taxonomy search
                            var accessions = assemblyResponse.Data.Reports
                                .Where(r => !string.IsNullOrEmpty(r.Accession))
                                .Select(r => r.Accession)
                                .Take(limit)
                                .ToList();

                            if (accessions.Any())
                            {
                                _logger.LogDebug("Making follow-up calls for {Count} accessions to get detailed data", accessions.Count);
                                
                                // Process in batches to avoid overwhelming the API
                                const int batchSize = 10;
                                for (int i = 0; i < accessions.Count; i += batchSize)
                                {
                                    var batch = accessions.Skip(i).Take(batchSize).ToList();
                                    
                                    try
                                    {
                                        // Use the accession-based assembly dataset reports endpoint which returns complete data
                                        var detailedResponse = await _ncbiClient.GetAssemblyDatasetReportsAsync(
                                            batch, 
                                            batchSize, 
                                            NCBI.Datasets.API.Models.Enums.AssemblyDatasetReportsRequestContentType.COMPLETE);
                                        
                                        if (detailedResponse.IsSuccess && detailedResponse.Data?.Reports != null)
                                        {
                                            // Only add the detailed reports, not the original taxonomy results
                                            foreach (var report in detailedResponse.Data.Reports)
                                            {
                                                var dataset = CreateDatasetViewModelFromReport(report, existingDownloads, currentDownloads);
                                                if (dataset != null)
                                                {
                                                    datasets.Add(dataset);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogWarning("Failed to get detailed assembly reports for batch: {Accessions}. Error: {Error}", 
                                                string.Join(",", batch), detailedResponse.ErrorMessage);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, "Error getting detailed data for batch: {Accessions}", string.Join(",", batch));
                                    }
                                    
                                    // Small delay between batches to be respectful to the API
                                    if (i + batchSize < accessions.Count)
                                    {
                                        await Task.Delay(100);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to get assembly reports for taxonomy IDs: {TaxonIds}. Error: {Error}", 
                                string.Join(",", taxonIds), assemblyResponse.ErrorMessage);
                        }
                    }
                }
                else
                {
                    // For default view, use specific well-known accessions directly (no taxonomy call needed)
                    var popularAccessions = new[] 
                    { 
                        "GCF_000005825.2",  // E. coli K-12 MG1655
                        "GCF_000001405.40", // Human GRCh38.p14
                        "GCF_000146045.2",  // S. cerevisiae S288C
                        "GCF_000001735.4",  // A. thaliana TAIR10.1
                        "GCF_000001215.4",  // D. melanogaster Release 6
                        "GCF_000002305.1",  // C. elegans WBcel235
                        "GCF_000009605.1",  // B. subtilis 168
                        "GCF_000006765.1",  // P. aeruginosa PAO1
                        "GCF_000195955.2",  // M. tuberculosis H37Rv
                        "GCF_000013425.1"   // S. aureus NCTC 8325
                    };
                    
                    _logger.LogDebug("Getting popular datasets using direct accessions: {Accessions}", string.Join(",", popularAccessions));
                    
                    try
                    {
                        // Get detailed information directly using accession-based assembly reports
                        var detailedResponse = await _ncbiClient.GetAssemblyDatasetReportsAsync(
                            popularAccessions.Take(10), 
                            10, 
                            NCBI.Datasets.API.Models.Enums.AssemblyDatasetReportsRequestContentType.COMPLETE);
                        
                        if (detailedResponse.IsSuccess && detailedResponse.Data?.Reports != null)
                        {
                            foreach (var report in detailedResponse.Data.Reports.Take(10))
                            {
                                var dataset = CreateDatasetViewModelFromReport(report, existingDownloads, currentDownloads);
                                if (dataset != null)
                                {
                                    datasets.Add(dataset);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to get detailed assembly reports for popular organisms. Error: {Error}", detailedResponse.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting detailed data for popular organisms");
                    }
                }

                _logger.LogInformation("Retrieved {Count} datasets from NCBI API", datasets.Count);
                return datasets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available datasets from NCBI");
                // Return empty list on error instead of fallback mock data
                return new List<AvailableDatasetViewModel>();
            }
        }

        /*! \fn CreateDatasetViewModelFromReport
         * \brief Create AvailableDatasetViewModel from AssemblyReport
         * \param report Assembly report from NCBI API
         * \param existingDownloads List of already downloaded accession IDs
         * \param currentDownloads List of currently downloading accession IDs
         * \return AvailableDatasetViewModel or null if report is invalid
         */
        private AvailableDatasetViewModel? CreateDatasetViewModelFromReport(
            object report,
            List<string> existingDownloads,
            List<string> currentDownloads)
        {
            try
            {
                // Cast to the proper type - we need to import the model
                if (report is not NCBI.Datasets.API.Models.Genome.AssemblyReport assemblyReport)
                {
                    _logger.LogWarning("Report is not of expected type AssemblyReport: {ActualType}", report?.GetType().Name ?? "null");
                    return null;
                }

                // Validate required fields
                if (string.IsNullOrEmpty(assemblyReport.Accession))
                {
                    _logger.LogWarning("Assembly report missing required accession");
                    return null;
                }

                // Create a mapping of known taxonomy IDs to organism names for better fallbacks
                var knownOrganisms = new Dictionary<int, string>
                {
                    { 511145, "Escherichia coli str. K-12 substr. MG1655" },
                    { 9606, "Homo sapiens" },
                    { 559292, "Saccharomyces cerevisiae S288C" },
                    { 3702, "Arabidopsis thaliana" },
                    { 7227, "Drosophila melanogaster" },
                    { 6239, "Caenorhabditis elegans" },
                    { 224308, "Bacillus subtilis subsp. subtilis str. 168" },
                    { 208964, "Pseudomonas aeruginosa PAO1" },
                    { 83332, "Mycobacterium tuberculosis H37Rv" },
                    { 158879, "Staphylococcus aureus subsp. aureus NCTC 8325" }
                };

                var dataset = new AvailableDatasetViewModel
                {
                    AccessionId = assemblyReport.Accession,
                    AssemblyName = !string.IsNullOrEmpty(assemblyReport.AssemblyName) ? 
                        assemblyReport.AssemblyName : 
                        $"Assembly {assemblyReport.Accession}",
                    OrganismName = !string.IsNullOrEmpty(assemblyReport.OrganismName) ? 
                        assemblyReport.OrganismName : 
                        (assemblyReport.Taxid > 0 && knownOrganisms.ContainsKey(assemblyReport.Taxid) ? 
                            knownOrganisms[assemblyReport.Taxid] : 
                            "Unknown Organism"),
                    TaxonomyId = assemblyReport.Taxid > 0 ? assemblyReport.Taxid : 0,
                    SpeciesId = assemblyReport.Taxid > 0 ? assemblyReport.Taxid : 0,
                    AssemblyLevel = !string.IsNullOrEmpty(assemblyReport.AssemblyLevel) ? 
                        assemblyReport.AssemblyLevel : 
                        "Unknown",
                    SubmissionDate = assemblyReport.SubmissionDate ?? assemblyReport.ReleaseDate,
                    IsAlreadyDownloaded = existingDownloads.Contains(assemblyReport.Accession),
                    IsCurrentlyDownloading = currentDownloads.Contains(assemblyReport.Accession),
                    EstimatedSize = assemblyReport.AssemblyStats?.TotalSequenceLength ?? 0,
                    ContigCount = assemblyReport.AssemblyStats?.NumberOfContigs ?? 0,
                    TotalLength = assemblyReport.AssemblyStats?.TotalSequenceLength ?? 0
                };

                _logger.LogDebug("Created dataset view model for accession: {Accession} with organism: {Organism}", 
                    assemblyReport.Accession, dataset.OrganismName);
                return dataset;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create dataset view model from report");
                return null;
            }
        }

        /*! \fn RequestDownloadAsync
         * \brief Request download of selected datasets
         * \param request Download request details
         * \param requestedBy User requesting the download
         * \return Job ID for tracking
         */
        public async Task<string> RequestDownloadAsync(DatasetDownloadRequestViewModel request, string requestedBy)
        {
            try
            {
                _logger.LogInformation("Processing download request for {Count} datasets by user {User}", 
                    request.AccessionIds.Count, requestedBy);

                var jobIds = new List<string>();

                foreach (var accessionId in request.AccessionIds)
                {
                    // Check if already downloading or downloaded
                    var existingDownload = await _context.DatasetDownloads
                        .FirstOrDefaultAsync(d => d.AccessionId == accessionId && 
                                                 (d.Status == DatasetDownloadStatus.Queued ||
                                                  d.Status == DatasetDownloadStatus.Downloading ||
                                                  d.Status == DatasetDownloadStatus.Processing ||
                                                  d.Status == DatasetDownloadStatus.Completed ||
                                                  d.Status == DatasetDownloadStatus.ReadyForBlast));

                    if (existingDownload != null)
                    {
                        _logger.LogWarning("Dataset {AccessionId} is already downloaded or in progress", accessionId);
                        continue;
                    }

                    // Get dataset details from NCBI (simplified for now)
                    var genomeRequest = new GenomeDownloadSummaryRequest 
                    { 
                        Accessions = new List<string> { accessionId } 
                    };
                    var genomeResponse = await _ncbiClient.GetGenomeDownloadSummaryAsync(genomeRequest);
                    
                    // For now, create a basic record since we don't have full genome reports
                    var download = new DatasetDownload
                    {
                        AccessionId = accessionId,
                        AssemblyName = $"Assembly {accessionId}",
                        OrganismName = "Unknown organism",
                        TaxonomyId = 0,
                        SpeciesId = 0,
                        Status = DatasetDownloadStatus.Queued,
                        Progress = 0,
                        IncludeAnnotations = request.IncludeAnnotations,
                        IncludeSequence = request.IncludeSequence,
                        RequestedBy = requestedBy,
                        FileSize = genomeResponse?.Data?.Hydrated?.EstimatedFileSizeMb * 1024 * 1024 ?? 0
                    };

                    _context.DatasetDownloads.Add(download);
                    await _context.SaveChangesAsync();

                    // Queue download job
                    var queueName = request.Priority switch
                    {
                        DownloadPriority.High => "downloads-high",
                        DownloadPriority.Low => "downloads-low",
                        _ => "downloads"
                    };

                    var jobId = BackgroundJob.Enqueue<DatasetDownloadJob>(
                        job => job.DownloadDatasetAsync(download.Id, JobCancellationToken.Null));

                    download.JobId = jobId;
                    await _context.SaveChangesAsync();

                    jobIds.Add(jobId);

                    _logger.LogInformation("Queued download job {JobId} for dataset {AccessionId}", jobId, accessionId);
                }

                return string.Join(",", jobIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing download request");
                throw;
            }
        }

        /*! \fn GetDownloadHistoryAsync
         * \brief Get download history
         * \return List of downloads
         */
        public async Task<List<DatasetDownload>> GetDownloadHistoryAsync()
        {
            return await _context.DatasetDownloads
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        /*! \fn GetDownloadStatusAsync
         * \brief Get status of specific download
         * \param downloadId Download ID
         * \return Download status
         */
        public async Task<DatasetDownload?> GetDownloadStatusAsync(int downloadId)
        {
            return await _context.DatasetDownloads
                .FirstOrDefaultAsync(d => d.Id == downloadId);
        }

        /*! \fn CancelDownloadAsync
         * \brief Cancel a download
         * \param downloadId Download ID
         */
        public async Task CancelDownloadAsync(int downloadId)
        {
            var download = await _context.DatasetDownloads.FindAsync(downloadId);
            if (download != null && !string.IsNullOrEmpty(download.JobId))
            {
                BackgroundJob.Delete(download.JobId);
                download.Status = DatasetDownloadStatus.Cancelled;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cancelled download {DownloadId} with job {JobId}", downloadId, download.JobId);
            }
        }

        /*! \fn RetryDownloadAsync
         * \brief Retry a failed download
         * \param downloadId Download ID
         */
        public async Task RetryDownloadAsync(int downloadId)
        {
            var download = await _context.DatasetDownloads.FindAsync(downloadId);
            if (download != null && download.Status == DatasetDownloadStatus.Failed)
            {
                download.Status = DatasetDownloadStatus.Queued;
                download.Progress = 0;
                download.ErrorMessage = string.Empty;
                download.StartedAt = null;
                download.CompletedAt = null;

                var jobId = BackgroundJob.Enqueue<DatasetDownloadJob>(
                    job => job.DownloadDatasetAsync(download.Id, JobCancellationToken.Null));

                download.JobId = jobId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Retrying download {DownloadId} with new job {JobId}", downloadId, jobId);
            }
        }
    }
}

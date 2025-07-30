using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NCBI.Datasets.API;
using NCBI.Datasets.API.Models.Genome;
using NCBI.Datasets.API.Services;
using Ribosoft.Data;
using Ribosoft.Models;

namespace Ribosoft.Jobs
{
    /*! \class DatasetDownloadJob
     * \brief Hangfire job for downloading NCBI datasets
     */
    public class DatasetDownloadJob
    {
        private readonly ApplicationDbContext _context;
        private readonly INCBIDatasetsClient _ncbiClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatasetDownloadJob> _logger;

        /*! \fn DatasetDownloadJob
         * \brief Constructor
         */
        public DatasetDownloadJob(
            DbContextOptions<ApplicationDbContext> dbOptions,
            INCBIDatasetsClient ncbiClient,
            IConfiguration configuration,
            ILogger<DatasetDownloadJob> logger)
        {
            _context = new ApplicationDbContext(dbOptions);
            _ncbiClient = ncbiClient;
            _configuration = configuration;
            _logger = logger;
        }

        /*! \fn DownloadDatasetAsync
         * \brief Download a dataset from NCBI
         * \param downloadId Database ID of the download record
         * \param cancellationToken Cancellation token
         */
        [Queue("downloads")]
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
        public async Task DownloadDatasetAsync(int downloadId, IJobCancellationToken cancellationToken)
        {
            var download = await _context.DatasetDownloads.FindAsync(downloadId);
            if (download == null)
            {
                _logger.LogError("Download record {DownloadId} not found", downloadId);
                return;
            }

            try
            {
                _logger.LogInformation("Starting download for dataset {AccessionId} (ID: {DownloadId})", 
                    download.AccessionId, downloadId);

                // Update status to downloading
                download.Status = DatasetDownloadStatus.Downloading;
                download.StartedAt = DateTime.UtcNow;
                download.Progress = 0;
                await _context.SaveChangesAsync();

                cancellationToken.ThrowIfCancellationRequested();

                // Step 1: Get download summary to get the download URL
                await UpdateProgress(download, 10, "Getting download information...");
                
                var request = new GenomeDownloadSummaryRequest
                {
                    Accessions = new List<string> { download.AccessionId }
                };

                // Build list of annotation types to include
                var annotationTypes = new List<NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType>();

                if (download.IncludeSequence)
                {
                    // Include FASTA sequence files
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.GENOME_FASTA);
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.RNA_FASTA);
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.PROT_FASTA);
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.CDS_FASTA);
                    
                    _logger.LogInformation("Including sequence data (FASTA files) for {AccessionId}", download.AccessionId);
                }

                if (download.IncludeAnnotations)
                {
                    // Include annotation files
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.GENOME_GFF);
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.GENOME_GBFF);
                    annotationTypes.Add(NCBI.Datasets.API.Models.Enums.AnnotationForAssemblyType.GENOME_GTF);
                    
                    _logger.LogInformation("Including annotation data (GFF/GBFF/GTF files) for {AccessionId}", download.AccessionId);
                }

                if (annotationTypes.Count > 0)
                {
                    request.IncludeAnnotationTypes = annotationTypes;
                }
                else
                {
                    _logger.LogWarning("No sequence or annotation data requested for {AccessionId} - download will only contain metadata", download.AccessionId);
                }

                var downloadSummary = await _ncbiClient.GetGenomeDownloadSummaryAsync(request);
                if (downloadSummary?.Data?.Hydrated?.Url == null)
                {
                    throw new InvalidOperationException($"Could not get download URL for {download.AccessionId}");
                }

                download.DownloadUrl = downloadSummary.Data.Hydrated.Url;
                download.FileSize = downloadSummary.Data.Hydrated.EstimatedFileSizeMb * 1024 * 1024;
                await _context.SaveChangesAsync();

                cancellationToken.ThrowIfCancellationRequested();

                // Step 2: Download the dataset file
                await UpdateProgress(download, 20, "Starting file download...");

                var downloadPath = GetDownloadPath(download.AccessionId);
                // Directory creation and permission checking is now handled in GetDownloadPath

                await DownloadFileWithProgress(download, downloadSummary.Data.Hydrated.Url, downloadPath, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                // Step 3: Verify download
                await UpdateProgress(download, 90, "Verifying download...");
                
                if (!File.Exists(downloadPath))
                {
                    throw new FileNotFoundException($"Downloaded file not found at {downloadPath}");
                }

                var fileInfo = new FileInfo(downloadPath);
                download.LocalPath = downloadPath;
                download.DownloadedBytes = fileInfo.Length;

                // Step 4: Complete
                await UpdateProgress(download, 100, "Download completed");
                
                download.Status = DatasetDownloadStatus.Completed;
                download.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully downloaded dataset {AccessionId} to {Path}", 
                    download.AccessionId, downloadPath);

                // Optionally trigger BLAST database creation
                if (ShouldCreateBlastDatabase())
                {
                    BackgroundJob.Enqueue<CreateBlastDatabaseJob>(
                        job => job.CreateBlastDatabaseAsync(downloadId, JobCancellationToken.Null));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Download cancelled for dataset {AccessionId}", download.AccessionId);
                download.Status = DatasetDownloadStatus.Cancelled;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading dataset {AccessionId}", download.AccessionId);
                
                download.Status = DatasetDownloadStatus.Failed;
                download.ErrorMessage = ex.Message;
                download.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                throw; // Re-throw for Hangfire retry mechanism
            }
        }

        /*! \fn DownloadFileWithProgress
         * \brief Download file with progress tracking
         */
        private async Task DownloadFileWithProgress(DatasetDownload download, string url, string filePath, 
            IJobCancellationToken cancellationToken)
        {
            const int bufferSize = 8192;
            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;

            var response = await _ncbiClient.DownloadGenomeDatasetAsync(url);
            if (response?.Data == null)
            {
                throw new InvalidOperationException("Failed to download dataset");
            }

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            var contentLength = response.Data.Length;
            
            // Write the byte array to file
            await fileStream.WriteAsync(response.Data, 0, response.Data.Length);
            totalBytesRead = response.Data.Length;

            // Update progress
            var progress = contentLength > 0 ? (int)((totalBytesRead * 70) / contentLength) + 20 : 90;
            await UpdateProgress(download, Math.Min(progress, 89), 
                $"Downloaded {FormatBytes(totalBytesRead)} of {FormatBytes(contentLength)}");

            download.DownloadedBytes = totalBytesRead;
            await _context.SaveChangesAsync();
        }

        /*! \fn UpdateProgress
         * \brief Update download progress
         */
        private async Task UpdateProgress(DatasetDownload download, int progress, string message)
        {
            download.Progress = progress;
            // You could add a StatusMessage property to track current operation
            await _context.SaveChangesAsync();
            
            _logger.LogDebug("Download {DownloadId} progress: {Progress}% - {Message}", 
                download.Id, progress, message);
        }

        /*! \fn GetDownloadPath
         * \brief Get local download path for dataset with enhanced error handling
         */
        private string GetDownloadPath(string accessionId)
        {
            var downloadDir = _configuration["Assemblies:Path"] ?? 
                             Path.Combine(Directory.GetCurrentDirectory(), "Downloads", "Datasets");
            
            // Expand ~ to home directory if present
            downloadDir = ExpandPath(downloadDir);
            
            try
            {
                // Check if directory exists first
                if (!Directory.Exists(downloadDir))
                {
                    _logger.LogInformation("Creating download directory: {DownloadDir}", downloadDir);
                    try
                    {
                        Directory.CreateDirectory(downloadDir);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogError(ex, "Permission denied creating download directory: {DownloadDir}", downloadDir);
                        throw new InvalidOperationException(
                            $"Permission denied creating download directory '{downloadDir}'. " +
                            $"Please ensure the application has permissions to create directories in the parent path. " +
                            $"You may need to run: sudo mkdir -p {downloadDir} && sudo chown -R $USER:$USER {downloadDir} && sudo chmod -R 755 {downloadDir}");
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        _logger.LogError(ex, "Parent directory path not found: {DownloadDir}", downloadDir);
                        throw new InvalidOperationException(
                            $"Parent directory path for '{downloadDir}' not found. " +
                            $"Please ensure the parent directories exist and are accessible. " +
                            $"You may need to run: sudo mkdir -p {Path.GetDirectoryName(downloadDir)} && sudo chmod 755 {Path.GetDirectoryName(downloadDir)}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error creating download directory: {DownloadDir}", downloadDir);
                        throw new InvalidOperationException(
                            $"Error creating download directory '{downloadDir}': {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogDebug("Download directory already exists: {DownloadDir}", downloadDir);
                }
                
                // Test write permissions by creating a temporary file
                var testFile = Path.Combine(downloadDir, $".test_{Guid.NewGuid()}.tmp");
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    _logger.LogDebug("Download directory permissions verified: {DownloadDir}", downloadDir);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Permission denied accessing existing download directory: {DownloadDir}", downloadDir);
                    throw new InvalidOperationException(
                        $"Permission denied accessing download directory '{downloadDir}'. " +
                        $"The directory exists but the application cannot write to it. " +
                        $"Please fix permissions: sudo chown -R $USER:$USER {downloadDir} && sudo chmod -R 755 {downloadDir}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot write to download directory: {DownloadDir}", downloadDir);
                    throw new InvalidOperationException(
                        $"Cannot write to download directory '{downloadDir}': {ex.Message}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied accessing download directory path: {DownloadDir}", downloadDir);
                throw new InvalidOperationException(
                    $"Permission denied accessing download directory '{downloadDir}'. " +
                    $"Please ensure the application has read access to check if the directory exists. " +
                    $"You may need to run: sudo chmod 755 {downloadDir}");
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError(ex, "Unexpected error accessing download directory: {DownloadDir}", downloadDir);
                throw new InvalidOperationException(
                    $"Error accessing download directory '{downloadDir}': {ex.Message}");
            }
            
            var filePath = Path.Combine(downloadDir, $"{accessionId}.zip");
            _logger.LogDebug("Download path for {AccessionId}: {FilePath}", accessionId, filePath);
            
            return filePath;
        }

        /*! \fn ExpandPath
         * \brief Expand ~ and environment variables in path
         */
        private static string ExpandPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Expand ~ to home directory
            if (path.StartsWith("~/") || path == "~")
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (path == "~")
                    return homeDir;
                return Path.Combine(homeDir, path.Substring(2));
            }

            // Expand environment variables
            return Environment.ExpandEnvironmentVariables(path);
        }

        /*! \fn ValidateDownloadConfiguration
         * \brief Validate download directory configuration and permissions
         * \return Validation result with any issues found
         */
        public static (bool IsValid, string ErrorMessage) ValidateDownloadConfiguration(IConfiguration configuration)
        {
            try
            {
                var downloadDir = configuration["Assemblies:Path"] ?? 
                                 Path.Combine(Directory.GetCurrentDirectory(), "Downloads", "Datasets");
                
                // Expand ~ to home directory if present
                downloadDir = ExpandPath(downloadDir);
                
                // Check if path is absolute and valid
                if (!Path.IsPathRooted(downloadDir))
                {
                    return (false, $"Download path '{downloadDir}' must be an absolute path");
                }
                
                // Check if directory exists first
                if (!Directory.Exists(downloadDir))
                {
                    // Try to create directory if it doesn't exist
                    try
                    {
                        Directory.CreateDirectory(downloadDir);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return (false, $"Permission denied creating download directory '{downloadDir}'. " +
                                      $"Run: sudo mkdir -p {downloadDir} && sudo chown -R $USER:$USER {downloadDir} && sudo chmod -R 755 {downloadDir}");
                    }
                    catch (DirectoryNotFoundException)
                    {
                        return (false, $"Parent directory path for '{downloadDir}' not found. " +
                                      $"Run: sudo mkdir -p {Path.GetDirectoryName(downloadDir)} && sudo chmod 755 {Path.GetDirectoryName(downloadDir)}");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Cannot create download directory '{downloadDir}': {ex.Message}");
                    }
                }
                
                // Test write permissions on existing directory
                var testFile = Path.Combine(downloadDir, $".permission_test_{Guid.NewGuid()}.tmp");
                try
                {
                    File.WriteAllText(testFile, "permission test");
                    File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    return (false, $"No write permission to existing download directory '{downloadDir}'. " +
                                  $"Run: sudo chown -R $USER:$USER {downloadDir} && sudo chmod -R 755 {downloadDir}");
                }
                catch (Exception ex)
                {
                    return (false, $"Cannot write to download directory '{downloadDir}': {ex.Message}");
                }
                
                return (true, string.Empty);
            }
            catch (UnauthorizedAccessException ex)
            {
                var downloadDir = configuration["Assemblies:Path"] ?? 
                                 Path.Combine(Directory.GetCurrentDirectory(), "Downloads", "Datasets");
                downloadDir = ExpandPath(downloadDir);
                return (false, $"Permission denied accessing download directory '{downloadDir}': {ex.Message}. " +
                              $"Run: sudo chmod 755 {downloadDir}");
            }
            catch (Exception ex)
            {
                return (false, $"Error validating download configuration: {ex.Message}");
            }
        }

        /*! \fn ShouldCreateBlastDatabase
         * \brief Check if BLAST database should be created automatically
         */
        private bool ShouldCreateBlastDatabase()
        {
            return _configuration.GetValue<bool>("Assemblies:AutoCreateBlastDatabase", true);
        }

        /*! \fn FormatBytes
         * \brief Format bytes for display
         */
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ribosoft.Data;
using Ribosoft.Models;

namespace Ribosoft.Jobs
{
    /*! \class CreateBlastDatabaseJob
     * \brief Hangfire job for creating BLAST databases from downloaded datasets
     */
    public class CreateBlastDatabaseJob
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateBlastDatabaseJob> _logger;

        /*! \fn CreateBlastDatabaseJob
         * \brief Constructor
         */
        public CreateBlastDatabaseJob(
            DbContextOptions<ApplicationDbContext> dbOptions,
            IConfiguration configuration,
            ILogger<CreateBlastDatabaseJob> logger)
        {
            _context = new ApplicationDbContext(dbOptions);
            _configuration = configuration;
            _logger = logger;
        }

        /*! \fn CreateBlastDatabaseAsync
         * \brief Create BLAST database from downloaded dataset
         * \param downloadId Database ID of the download record
         * \param cancellationToken Cancellation token
         */
        [Queue("blast")]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 900 })]
        public async Task CreateBlastDatabaseAsync(int downloadId, IJobCancellationToken cancellationToken)
        {
            var download = await _context.DatasetDownloads.FindAsync(downloadId);
            if (download == null)
            {
                _logger.LogError("Download record {DownloadId} not found", downloadId);
                return;
            }

            if (download.Status != DatasetDownloadStatus.Completed)
            {
                _logger.LogWarning("Download {DownloadId} is not completed, cannot create BLAST database", downloadId);
                return;
            }

            try
            {
                _logger.LogInformation("Starting BLAST database creation for dataset {AccessionId} (ID: {DownloadId})", 
                    download.AccessionId, downloadId);

                // Update status
                download.Status = DatasetDownloadStatus.Processing;
                await _context.SaveChangesAsync();

                cancellationToken.ThrowIfCancellationRequested();

                // Step 1: Extract the downloaded ZIP file
                var extractPath = await ExtractDataset(download, cancellationToken);

                // Step 2: Find FASTA files in the extracted content
                var fastaFiles = FindFastaFiles(extractPath);
                if (fastaFiles.Count == 0)
                {
                    throw new InvalidOperationException($"No FASTA files found in dataset {download.AccessionId}");
                }

                // Step 3: Create BLAST database for each FASTA file (if enabled)
                var blastDbPath = _configuration["Blast:BLASTDB"] ?? 
                                 Path.Combine(Directory.GetCurrentDirectory(), "BlastDatabases");
                blastDbPath = ExpandPath(blastDbPath);
                Directory.CreateDirectory(blastDbPath);
                
                var createBlastDb = _configuration.GetValue<bool>("DatasetDownloads:CreateBlastDatabase", true);
                
                if (!createBlastDb)
                {
                    _logger.LogInformation("BLAST database creation is disabled, skipping for {AccessionId}", download.AccessionId);
                }
                else
                {
                    // Check if BLAST database creation should be skipped for large files
                    var maxFileSizeMB = _configuration.GetValue<double>("DatasetDownloads:MaxBlastDbFileSizeMB", 500);
                    var skipLargeFiles = _configuration.GetValue<bool>("DatasetDownloads:SkipBlastDbForLargeFiles", false);

                    var processedFiles = 0;
                    var skippedFiles = 0;
                    var totalFiles = fastaFiles.Count;

                    _logger.LogInformation("Starting BLAST database creation for {TotalFiles} FASTA files", totalFiles);

                    foreach (var fastaFile in fastaFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var fileInfo = new FileInfo(fastaFile);
                        var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
                        
                        if (skipLargeFiles && fileSizeMB > maxFileSizeMB)
                        {
                            _logger.LogWarning("Skipping BLAST database creation for large file {FileName} ({SizeMB:F1} MB > {MaxSizeMB} MB limit)", 
                                Path.GetFileName(fastaFile), fileSizeMB, maxFileSizeMB);
                            skippedFiles++;
                            continue;
                        }
                        
                        // Update progress
                        var progressPercent = 70 + (processedFiles * 20 / totalFiles); // 70-90% range for BLAST creation
                        await UpdateDownloadProgress(download, progressPercent, 
                            $"Creating BLAST database {processedFiles + 1}/{totalFiles}: {Path.GetFileName(fastaFile)}");
                        
                        await CreateBlastDatabase(fastaFile, blastDbPath, download.AccessionId, cancellationToken);
                        processedFiles++;
                    }

                    _logger.LogInformation("BLAST database creation completed: {ProcessedFiles} processed, {SkippedFiles} skipped", 
                        processedFiles, skippedFiles);
                }

                // Step 4: Create Assembly record for integration with existing system
                await CreateAssemblyRecord(download, blastDbPath);

                // Step 5: Clean up extracted files and ZIP file
                var cleanupExtracted = _configuration.GetValue<bool>("DatasetDownloads:CleanupAfterProcessing", true);
                var cleanupZip = _configuration.GetValue<bool>("DatasetDownloads:CleanupZipFiles", true);
                
                if (cleanupExtracted)
                {
                    try
                    {
                        Directory.Delete(extractPath, true);
                        _logger.LogInformation("Cleaned up extracted files at {ExtractPath}", extractPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clean up extracted files at {ExtractPath}", extractPath);
                    }
                }
                
                if (cleanupZip && !string.IsNullOrEmpty(download.LocalPath) && File.Exists(download.LocalPath))
                {
                    try
                    {
                        File.Delete(download.LocalPath);
                        _logger.LogInformation("Cleaned up ZIP file at {ZipPath}", download.LocalPath);
                        
                        // Clear the local path since file is deleted
                        download.LocalPath = null;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clean up ZIP file at {ZipPath}", download.LocalPath);
                    }
                }

                // Step 6: Complete
                download.Status = DatasetDownloadStatus.ReadyForBlast;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created BLAST database for dataset {AccessionId}", 
                    download.AccessionId);

                // Trigger assembly database rescan to pick up the new database
                BackgroundJob.Enqueue<UpdateAssemblyDatabase>(
                    job => job.Rescan(JobCancellationToken.Null));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("BLAST database creation cancelled for dataset {AccessionId}", download.AccessionId);
                download.Status = DatasetDownloadStatus.Cancelled;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating BLAST database for dataset {AccessionId}", download.AccessionId);
                
                download.Status = DatasetDownloadStatus.Failed;
                download.ErrorMessage = $"BLAST database creation failed: {ex.Message}";
                await _context.SaveChangesAsync();
                
                throw; // Re-throw for Hangfire retry mechanism
            }
        }

        /*! \fn ExtractDataset
         * \brief Extract downloaded ZIP file
         */
        private async Task<string> ExtractDataset(DatasetDownload download, IJobCancellationToken cancellationToken)
        {
            var extractPath = Path.Combine(
                Path.GetDirectoryName(download.LocalPath)!,
                Path.GetFileNameWithoutExtension(download.LocalPath));

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            Directory.CreateDirectory(extractPath);

            _logger.LogInformation("Extracting dataset {AccessionId} to {ExtractPath}", 
                download.AccessionId, extractPath);

            await Task.Run(() =>
            {
                using var archive = ZipFile.OpenRead(download.LocalPath);
                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        var destinationPath = Path.Combine(extractPath, entry.FullName);
                        var destinationDir = Path.GetDirectoryName(destinationPath);
                        
                        if (!string.IsNullOrEmpty(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }
                        
                        entry.ExtractToFile(destinationPath, true);
                    }
                }
            });

            return extractPath;
        }

        /*! \fn FindFastaFiles
         * \brief Find FASTA files in extracted dataset
         */
        private List<string> FindFastaFiles(string extractPath)
        {
            var fastaFiles = new List<string>();
            // Include all common FASTA extensions used by NCBI
            var fastaExtensions = new[] { ".fna", ".fasta", ".fa", ".fas", ".faa" };

            foreach (var file in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();
                if (fastaExtensions.Contains(extension))
                {
                    fastaFiles.Add(file);
                    _logger.LogDebug("Found FASTA file: {FilePath}", file);
                }
            }

            _logger.LogInformation("Found {Count} FASTA files in dataset {ExtractPath}", fastaFiles.Count, extractPath);
            
            if (fastaFiles.Count == 0)
            {
                // Log directory contents for debugging
                _logger.LogWarning("No FASTA files found. Directory contents:");
                LogDirectoryContents(extractPath);
            }
            
            return fastaFiles;
        }

        /*! \fn LogDirectoryContents
         * \brief Log directory contents for debugging
         */
        private void LogDirectoryContents(string path, int maxDepth = 3, int currentDepth = 0)
        {
            if (currentDepth >= maxDepth) return;
            
            try
            {
                var indent = new string(' ', currentDepth * 2);
                
                foreach (var dir in Directory.GetDirectories(path))
                {
                    _logger.LogWarning("{Indent}DIR: {DirName}", indent, Path.GetFileName(dir));
                    LogDirectoryContents(dir, maxDepth, currentDepth + 1);
                }
                
                foreach (var file in Directory.GetFiles(path))
                {
                    var fileInfo = new FileInfo(file);
                    _logger.LogWarning("{Indent}FILE: {FileName} ({Size} bytes)", 
                        indent, Path.GetFileName(file), fileInfo.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging directory contents for {Path}", path);
            }
        }

        /*! \fn CreateBlastDatabase
         * \brief Create BLAST database from FASTA file with timeout and progress monitoring
         */
        private async Task CreateBlastDatabase(string fastaFile, string blastDbPath, string accessionId, IJobCancellationToken? cancellationToken = null)
        {
            var fileName = Path.GetFileNameWithoutExtension(fastaFile);
            var dbName = $"{accessionId}_{fileName}";
            
            // Create accession-specific directory for better organization
            var accessionDbPath = Path.Combine(blastDbPath, accessionId);
            Directory.CreateDirectory(accessionDbPath);
            
            var dbPath = Path.Combine(accessionDbPath, dbName);

            // Get file size for progress estimation
            var fileInfo = new FileInfo(fastaFile);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            
            _logger.LogInformation("Creating BLAST database {DbName} from {FastaFile} ({SizeMB:F1} MB) in {AccessionDbPath}", 
                dbName, Path.GetFileName(fastaFile), fileSizeMB, accessionDbPath);

            // Check if database already exists
            if (File.Exists($"{dbPath}.nhr") && File.Exists($"{dbPath}.nin") && File.Exists($"{dbPath}.nsq"))
            {
                _logger.LogInformation("BLAST database {DbName} already exists, skipping creation", dbName);
                return;
            }

            var makeblastdbPath = _configuration["Blast:MakeBlastDbPath"] ?? "makeblastdb";
            
            // Determine database type based on file content/name
            var dbType = DetermineDbType(fastaFile, fileName);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = makeblastdbPath,
                Arguments = $"-in \"{fastaFile}\" -dbtype {dbType} -out \"{dbPath}\" -title \"{dbName}\" -parse_seqids",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            
            // Set up timeout based on file size (rough estimate: 1 minute per 10MB + 5 minute base)
            var timeoutMinutes = Math.Max(5, (int)(fileSizeMB / 10) + 5);
            var timeout = TimeSpan.FromMinutes(timeoutMinutes);
            
            _logger.LogInformation("Starting makeblastdb for {DbName} with {TimeoutMinutes} minute timeout", 
                dbName, timeoutMinutes);

            var startTime = DateTime.UtcNow;
            process.Start();

            // Monitor process with cancellation and timeout
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            try
            {
                // Create a combined cancellation token for timeout and job cancellation
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    timeoutCts.Token, 
                    cancellationToken?.ShutdownToken ?? CancellationToken.None);

                // Wait for process completion with cancellation
                await process.WaitForExitAsync(combinedCts.Token);
                
                var output = await outputTask;
                var error = await errorTask;
                var duration = DateTime.UtcNow - startTime;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"makeblastdb failed for {dbName} with exit code {process.ExitCode} after {duration:mm\\:ss}. " +
                        $"Error: {error}. Output: {output}");
                }

                _logger.LogInformation("Successfully created BLAST database {DbName} in {Duration:mm\\:ss} at {DbPath}", 
                    dbName, duration, accessionDbPath);
            }
            catch (OperationCanceledException) when (cancellationToken?.ShutdownToken.IsCancellationRequested == true)
            {
                _logger.LogWarning("BLAST database creation for {DbName} was cancelled by job cancellation", dbName);
                
                if (!process.HasExited)
                {
                    process.Kill(true);
                    await process.WaitForExitAsync();
                }
                throw;
            }
            catch (OperationCanceledException)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError("BLAST database creation for {DbName} timed out after {Duration:mm\\:ss} (limit: {TimeoutMinutes} minutes)", 
                    dbName, duration, timeoutMinutes);
                
                if (!process.HasExited)
                {
                    process.Kill(true);
                    await process.WaitForExitAsync();
                }
                
                throw new TimeoutException(
                    $"BLAST database creation for {dbName} timed out after {duration:mm\\:ss}. " +
                    $"Large datasets may require more time. Consider increasing the timeout or processing smaller files.");
            }
        }

        /*! \fn UpdateDownloadProgress
         * \brief Update download progress and status message
         */
        private async Task UpdateDownloadProgress(DatasetDownload download, int progress, string message)
        {
            download.Progress = progress;
            // Note: DatasetDownload model would need a StatusMessage property to store this
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Download {DownloadId} progress: {Progress}% - {Message}", 
                download.Id, progress, message);
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

        /*! \fn DetermineDbType
         * \brief Determine BLAST database type based on file content
         */
        private string DetermineDbType(string fastaFile, string fileName)
        {
            // Protein files typically use .faa extension or contain "protein" in name
            if (Path.GetExtension(fastaFile).ToLowerInvariant() == ".faa" || 
                fileName.ToLowerInvariant().Contains("protein"))
            {
                return "prot";
            }
            
            // Default to nucleotide for genomic, RNA, CDS files
            return "nucl";
        }

        /*! \fn CreateAssemblyRecord
         * \brief Create Assembly record for integration with existing system
         */
        private async Task CreateAssemblyRecord(DatasetDownload download, string blastDbPath)
        {
            // Check if assembly already exists
            var existingAssembly = await _context.Assemblies
                .FirstOrDefaultAsync(a => a.TaxonomyId == download.TaxonomyId);

            // Use organized path structure (accession-specific directory)
            var assemblyPath = Path.Combine(blastDbPath, download.AccessionId);

            if (existingAssembly != null)
            {
                _logger.LogInformation("Assembly record already exists for taxonomy {TaxonomyId}, updating", 
                    download.TaxonomyId);
                
                // Update existing record
                existingAssembly.AccessionId = download.AccessionId;
                existingAssembly.AssemblyName = download.AssemblyName;
                existingAssembly.OrganismName = download.OrganismName;
                existingAssembly.SpeciesId = download.SpeciesId;
                existingAssembly.Type = "Downloaded";
                existingAssembly.Path = assemblyPath;
                existingAssembly.IsEnabled = true;
                existingAssembly.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _logger.LogInformation("Creating new assembly record for taxonomy {TaxonomyId}", 
                    download.TaxonomyId);
                
                // Create new assembly record
                var assembly = new Assembly
                {
                    AccessionId = download.AccessionId,
                    AssemblyName = download.AssemblyName ?? download.AccessionId,
                    OrganismName = download.OrganismName ?? "Unknown organism",
                    SpeciesId = download.SpeciesId,
                    TaxonomyId = download.TaxonomyId,
                    Type = "Downloaded",
                    Path = assemblyPath,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Assemblies.Add(assembly);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Assembly record updated for {AccessionId} with path {AssemblyPath}", 
                download.AccessionId, assemblyPath);
        }
    }
}

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

                // Step 3: Create BLAST database for each FASTA file
                var blastDbPath = _configuration["Blast:BLASTDB"] ?? 
                                 Path.Combine(Directory.GetCurrentDirectory(), "BlastDatabases");
                
                Directory.CreateDirectory(blastDbPath);

                foreach (var fastaFile in fastaFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await CreateBlastDatabase(fastaFile, blastDbPath, download.AccessionId);
                }

                // Step 4: Create Assembly record for integration with existing system
                await CreateAssemblyRecord(download, blastDbPath);

                // Step 5: Clean up extracted files (optional)
                if (_configuration.GetValue<bool>("DatasetDownloads:CleanupAfterProcessing", true))
                {
                    Directory.Delete(extractPath, true);
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
         * \brief Create BLAST database from FASTA file
         */
        private async Task CreateBlastDatabase(string fastaFile, string blastDbPath, string accessionId)
        {
            var fileName = Path.GetFileNameWithoutExtension(fastaFile);
            var dbName = $"{accessionId}_{fileName}";
            var dbPath = Path.Combine(blastDbPath, dbName);

            _logger.LogInformation("Creating BLAST database {DbName} from {FastaFile}", dbName, fastaFile);

            var makeblastdbPath = _configuration["Blast:MakeBlastDbPath"] ?? "makeblastdb";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = makeblastdbPath,
                Arguments = $"-in \"{fastaFile}\" -dbtype nucl -out \"{dbPath}\" -title \"{dbName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"makeblastdb failed with exit code {process.ExitCode}. Error: {error}");
            }

            _logger.LogInformation("Successfully created BLAST database {DbName}", dbName);
        }

        /*! \fn CreateAssemblyRecord
         * \brief Create Assembly record for integration with existing system
         */
        private async Task CreateAssemblyRecord(DatasetDownload download, string blastDbPath)
        {
            // Check if assembly already exists
            var existingAssembly = await _context.Assemblies
                .FirstOrDefaultAsync(a => a.TaxonomyId == download.TaxonomyId);

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
                existingAssembly.Path = Path.Combine(blastDbPath, download.AccessionId);
                existingAssembly.IsEnabled = true;
                existingAssembly.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _logger.LogInformation("Creating new Assembly record for taxonomy {TaxonomyId}", 
                    download.TaxonomyId);
                
                // Create new assembly record
                var assembly = new Assembly
                {
                    TaxonomyId = download.TaxonomyId,
                    AccessionId = download.AccessionId,
                    AssemblyName = download.AssemblyName,
                    OrganismName = download.OrganismName,
                    SpeciesId = download.SpeciesId,
                    Type = "Downloaded",
                    Path = Path.Combine(blastDbPath, download.AccessionId),
                    IsEnabled = true
                };

                _context.Assemblies.Add(assembly);
            }

            await _context.SaveChangesAsync();
        }
    }
}

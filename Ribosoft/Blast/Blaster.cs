using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Ribosoft.Blast
{
    /*! \class Blaster
     * \brief Object class for the blaster functionality
     */
    public class Blaster
    {
        /*! \property Parameters
         * \brief Parameters used for the BLAST command
         */
        public BlastParameters Parameters { get; set; }

        /*!
         * \brief Default constructor
         */
        public Blaster()
        {
            Parameters = new BlastParameters();
        }

        /*! \fn IsAvailable
         * \brief Function to check that BLAST command line tools are available
         * \return Boolean for availability
         */
        public bool IsAvailable()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "blastn",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            string output;

            try
            {
                process.Start();

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }
            catch (Win32Exception)
            {
                // No such file
                return false;
            }
            finally
            {
                process.Close();
            }

            // primitive check that this is indeed blastn
            if (!output.StartsWith("blastn:", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        /*! \fn GetAvailableDatabases
         * \brief Returns list of BLAST databases available for use
         * \param path Path to the databases
         * \return List of available databases
         */
        public IList<Database> GetAvailableDatabases(string path)
        {
            var databases = new List<Database>();
            
            // First, get the basic database list
            var basicDatabases = GetBasicDatabaseList(path);
            
            // Group databases by taxonomy ID to consolidate multiple files for the same organism
            var groupedDatabases = new Dictionary<int, Database>();
            
            foreach (var basicDb in basicDatabases)
            {
                // Try to get taxonomic information for this database
                var taxInfo = GetDatabaseTaxonomicInfo(basicDb.AbsolutePath);
                if (taxInfo != null && taxInfo.TaxonomyId > 0)
                {
                    basicDb.TaxonomyId = taxInfo.TaxonomyId;
                    basicDb.SpeciesTaxonomyId = taxInfo.TaxonomyId; // Use same for now
                    basicDb.OrganismName = taxInfo.ScientificName ?? basicDb.OrganismName;
                }
                else
                {
                    // Fallback: use hash of accession for consistent taxonomy ID
                    if (!string.IsNullOrEmpty(basicDb.AccessionId))
                    {
                        basicDb.TaxonomyId = Math.Abs(basicDb.AccessionId.GetHashCode()) % 1000000; // Keep it reasonable
                        basicDb.SpeciesTaxonomyId = basicDb.TaxonomyId;
                    }
                }
                
                // Group by taxonomy ID to consolidate multiple database files
                if (groupedDatabases.ContainsKey(basicDb.TaxonomyId))
                {
                    var existing = groupedDatabases[basicDb.TaxonomyId];
                    
                    // Combine types (deduplicated)
                    var existingTypes = existing.Type.Split(',').Select(t => t.Trim()).ToHashSet();
                    existingTypes.Add(basicDb.Type);
                    existing.Type = string.Join(", ", existingTypes.Where(t => !string.IsNullOrEmpty(t)));
                    
                    // Combine paths
                    existing.RelativePath += " " + basicDb.RelativePath;
                    
                    // Update statistics (sum up)
                    existing.Nucleotides += basicDb.Nucleotides;
                    existing.Sequences += basicDb.Sequences;
                    existing.Bytes += basicDb.Bytes;
                    
                    // Keep the most recent update date
                    if (basicDb.UpdatedAt > existing.UpdatedAt)
                    {
                        existing.UpdatedAt = basicDb.UpdatedAt;
                    }
                }
                else
                {
                    // First database for this taxonomy ID - no need to set Path since it's not in Database model
                    groupedDatabases[basicDb.TaxonomyId] = basicDb;
                }
            }
            
            databases.AddRange(groupedDatabases.Values);
            return databases;
        }

        /*! \fn GetBasicDatabaseList
         * \brief Gets basic database information using -list option
         * \param path Path to the databases
         * \return List of basic database information
         */
        private IList<Database> GetBasicDatabaseList(string path)
        {
            var args = string.Format("-list {0} -recursive -list_outfmt \"%f\\t%p\\t%t\\t%d\\t%l\\t%n\\t%U\\t%v\"", EncodeParameterArgument(path));
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "blastdbcmd",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var databases = new List<Database>();
            
            // Regex patterns for extracting metadata from database titles and paths
            var accessionRegex = new Regex(@"GCF_\d+\.\d+|GCA_\d+\.\d+", RegexOptions.IgnoreCase);
            var organismRegex = new Regex(@"^([^[]+)", RegexOptions.IgnoreCase);
            var assemblyRegex = new Regex(@"\[([^\]]+)\]", RegexOptions.IgnoreCase);

            try
            {
                process.Start();
                
                string? outputLine;
                while ((outputLine = process.StandardOutput.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(outputLine))
                    {
                        continue;
                    }
                    
                    // Replace literal \t with actual tabs since blastdbcmd seems to output literal \t
                    outputLine = outputLine.Replace("\\t", "\t");
                    
                    var columns = outputLine.Split('\t');

                    if (columns.Length < 7)
                    {
                        continue;
                    }

                    var database = new Database
                    {
                        AbsolutePath = columns[0],
                        RelativePath = columns[0].Length > path.Length ? columns[0].Substring(path.Length + 1) : columns[0],
                        Type = columns[1], // Nucleotide or Protein
                        UpdatedAt = DateTime.TryParse(columns[3], out var updateDate) ? updateDate : DateTime.MinValue,
                        Nucleotides = BigInteger.TryParse(columns[4], out var nucleotides) ? nucleotides : BigInteger.Zero,
                        Sequences = BigInteger.TryParse(columns[5], out var sequences) ? sequences : BigInteger.Zero,
                        Bytes = BigInteger.TryParse(columns[6], out var bytes) ? bytes : BigInteger.Zero
                    };

                    var title = columns[2];
                    
                    // Extract accession ID from title or path
                    var accessionMatch = accessionRegex.Match(title);
                    if (!accessionMatch.Success)
                    {
                        accessionMatch = accessionRegex.Match(database.AbsolutePath);
                    }
                    if (accessionMatch.Success)
                    {
                        database.AccessionId = accessionMatch.Value;
                    }

                    // Extract organism name (everything before the first bracket)
                    var organismMatch = organismRegex.Match(title);
                    if (organismMatch.Success)
                    {
                        database.OrganismName = organismMatch.Groups[1].Value.Trim();
                    }

                    // Extract assembly name from brackets
                    var assemblyMatches = assemblyRegex.Matches(title);
                    if (assemblyMatches.Count > 0)
                    {
                        database.AssemblyName = assemblyMatches[0].Groups[1].Value;
                    }

                    // Set default values if not found
                    if (string.IsNullOrEmpty(database.OrganismName))
                    {
                        database.OrganismName = "Unknown organism";
                    }
                    if (string.IsNullOrEmpty(database.AssemblyName))
                    {
                        database.AssemblyName = database.AccessionId ?? "Unknown assembly";
                    }
                    
                    databases.Add(database);
                }
                
                string? errorLine;
                while ((errorLine = process.StandardError.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(errorLine))
                    {
                        Console.WriteLine($"blastdbcmd list error: {errorLine}");
                    }
                }
                
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running blastdbcmd list: {ex.Message}");
            }
            finally
            {
                process?.Close();
            }

            return databases;
        }

        /*! \fn GetDatabaseTaxonomicInfo
         * \brief Gets taxonomic information for a specific database
         * \param databasePath Path to the database
         * \return Taxonomic information or null if not available
         */
        private TaxonomicInfo? GetDatabaseTaxonomicInfo(string databasePath)
        {
            var args = string.Format("-db {0} -tax_info -outfmt \"%T\\t%S\\t%L\\t%K\\t%B\"", EncodeParameterArgument(databasePath));
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "blastdbcmd",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                
                string? outputLine;
                while ((outputLine = process.StandardOutput.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(outputLine) || outputLine.StartsWith("#"))
                    {
                        continue; // Skip comments and empty lines
                    }
                    
                    // Replace literal \t with actual tabs
                    outputLine = outputLine.Replace("\\t", "\t");
                    
                    var columns = outputLine.Split('\t');
                    if (columns.Length >= 2)
                    {
                        var taxInfo = new TaxonomicInfo();
                        
                        if (int.TryParse(columns[0], out var taxId) && taxId > 0)
                        {
                            taxInfo.TaxonomyId = taxId;
                        }
                        
                        if (columns.Length > 1 && !string.IsNullOrWhiteSpace(columns[1]) && columns[1] != "N/A")
                        {
                            taxInfo.ScientificName = columns[1];
                        }
                        
                        if (columns.Length > 2 && !string.IsNullOrWhiteSpace(columns[2]) && columns[2] != "N/A")
                        {
                            taxInfo.CommonName = columns[2];
                        }
                        
                        if (columns.Length > 3 && !string.IsNullOrWhiteSpace(columns[3]) && columns[3] != "N/A")
                        {
                            taxInfo.SuperKingdom = columns[3];
                        }
                        
                        if (columns.Length > 4 && !string.IsNullOrWhiteSpace(columns[4]) && columns[4] != "N/A")
                        {
                            taxInfo.BlastName = columns[4];
                        }
                        
                        // Only return if we got a valid taxonomy ID
                        if (taxInfo.TaxonomyId > 0)
                        {
                            process.WaitForExit();
                            return taxInfo;
                        }
                    }
                }
                
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting taxonomic info for {databasePath}: {ex.Message}");
            }
            finally
            {
                process?.Close();
            }

            return null;
        }

        /*! \class TaxonomicInfo
         * \brief Helper class to hold taxonomic information
         */
        private class TaxonomicInfo
        {
            public int TaxonomyId { get; set; }
            public string? ScientificName { get; set; }
            public string? CommonName { get; set; }
            public string? SuperKingdom { get; set; }
            public string? BlastName { get; set; }
        }
        
        /*!
         * \brief Wrapper function to call run
         * \return stdout string
         */
        public string Run()
        {
            return Run(this.Parameters);
        }

        /*!
         * \brief Function to run the BLAST command
         * \param parameters Parameters to use for the BLAST command
         * \return stdout string
         */
        public string Run(BlastParameters parameters)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "blastn",
                    Arguments = parameters.ToString(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            string output;

            try
            {
                process.Start();

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }
            catch (Win32Exception)
            {
                // No such file
                return string.Empty;
            }
            finally
            {
                process.Close();
            }

            return output;
        }

        /*!
         * \brief Function to encode parameter arguments
         * \param argument Argument to encode
         * \return Encoded argument
         */
        private static string EncodeParameterArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return argument;

            return "\"" + argument.Replace("\"", "\"\"") + "\"";
        }
    }
}

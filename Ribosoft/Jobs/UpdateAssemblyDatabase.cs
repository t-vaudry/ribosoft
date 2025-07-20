using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ribosoft.Blast;
using Ribosoft.Data;
using Ribosoft.Models;

namespace Ribosoft.Jobs
{
    /*! \class UpdateAssemblyDatabase
     * \brief Job class for updating the assemply database
     */
    public class UpdateAssemblyDatabase
    {
        /*! \property _configuration
         * \brief Local application configuration
         */
        private readonly IConfiguration _configuration;

        /*! \property _db
         * \brief Local application database context
         */
        private readonly ApplicationDbContext _db;

        /*! \fn UpdateAssemblyDatabase
         * \brief Default constructor
         * \param options Application database context
         * \param configuration Application configuration
         */
        public UpdateAssemblyDatabase(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        {
            _db =  new ApplicationDbContext(options);
            _configuration = configuration;
        }

        /*! \fn Rescan
         * \brief Rescan the configuration path for more assembly databases
         * \param cancellationToken Cancellation token
         * \return List of assemblies
         */
        [Queue("blast")]
        [AutomaticRetry(Attempts = 0)]
        public async Task Rescan(IJobCancellationToken cancellationToken)
        {
            var blaster = new Blaster();
            var availableDatabases = blaster.GetAvailableDatabases(_configuration["Blast:BLASTDB"] ?? "");
            var currentAssemblies = await _db.Assemblies.ToDictionaryAsync(x => x.TaxonomyId, x => x);
            
            cancellationToken.ThrowIfCancellationRequested();

            // Set all current assemblies as unavailable initially
            // We'll re-enable them as we discover them in the scan
            foreach (var assembly in currentAssemblies.Values)
            {
                assembly.IsEnabled = false;
                assembly.Type = string.Empty;
                assembly.Path = string.Empty;
            }

            foreach (var database in availableDatabases)
            {
                if (currentAssemblies.ContainsKey(database.TaxonomyId))
                {
                    // Update the existing assembly with the same TaxonomyId
                    var assembly = currentAssemblies[database.TaxonomyId];
                    assembly.AccessionId = database.AccessionId;
                    assembly.AssemblyName = database.AssemblyName;
                    assembly.OrganismName = database.OrganismName;
                    assembly.SpeciesId = database.SpeciesTaxonomyId;
                    assembly.Type = database.Type;
                    assembly.Path = database.RelativePath;
                    assembly.IsEnabled = true;
                }
                else
                {
                    // Create a new assembly for this TaxonomyId
                    var assembly = new Assembly
                    {
                        TaxonomyId = database.TaxonomyId,
                        AccessionId = database.AccessionId,
                        AssemblyName = database.AssemblyName,
                        OrganismName = database.OrganismName,
                        SpeciesId = database.SpeciesTaxonomyId,
                        Type = database.Type,
                        Path = database.RelativePath,
                        IsEnabled = true
                    };
                    
                    _db.Assemblies.Add(assembly);
                    currentAssemblies[database.TaxonomyId] = assembly;
                }
            }
            
            cancellationToken.ThrowIfCancellationRequested();

            await _db.SaveChangesAsync();
        }
    }
}
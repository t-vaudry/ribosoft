using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ribosoft.Blast;
using Ribosoft.Data;
using Ribosoft.Models;

namespace Ribosoft.Jobs
{
    public class UpdateAssemblyDatabase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        
        public UpdateAssemblyDatabase(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        {
            _db =  new ApplicationDbContext(options);
            _configuration = configuration;
        }
        
        [Queue("blast")]
        [AutomaticRetry(Attempts = 0)]
        public async Task Rescan(IJobCancellationToken cancellationToken)
        {
            var blaster = new Blaster();
            var availableDatabases = blaster.GetAvailableDatabases(_configuration["Blast:BLASTDB"]);
            var currentAssemblies = await _db.Assemblies.ToDictionaryAsync(x => x.TaxonomyId, x => x);
            
            cancellationToken.ThrowIfCancellationRequested();

            // set all current assemblies as unavailable, we'll assume they were deleted and will reset them as enabled as we discover them
            foreach (var assembly in currentAssemblies)
            {
                assembly.Value.IsEnabled = false;
                assembly.Value.Type = string.Empty;
                assembly.Value.Path = string.Empty;
            }

            foreach (var database in availableDatabases)
            {
                if (currentAssemblies.ContainsKey(database.TaxonomyId))
                {
                    // update the assembly we already have for the taxid
                    var assembly = currentAssemblies[database.TaxonomyId];
                    assembly.AssemblyName = database.AssemblyName;
                    assembly.OrganismName = database.OrganismName;
                    assembly.SpeciesId = database.SpeciesTaxonomyId;
                    assembly.AccessionId = database.AccessionId;
                    assembly.IsEnabled = true;
                    
                    if (!string.IsNullOrEmpty(assembly.Type))
                    {
                        assembly.Type += ',';
                        assembly.Path += ' ';
                    }

                    assembly.Type += database.Type;
                    assembly.Path += database.RelativePath;
                }
                else
                {
                    // create a new assembly for the taxid
                    var assembly = new Assembly
                    {
                        TaxonomyId = database.TaxonomyId,
                        AssemblyName = database.AssemblyName,
                        OrganismName = database.OrganismName,
                        SpeciesId = database.SpeciesTaxonomyId,
                        AccessionId = database.AccessionId,
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
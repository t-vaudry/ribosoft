using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ribosoft.Data
{
    public class NpgsqlDbContext : ApplicationDbContext
    {
        public NpgsqlDbContext(DbContextOptions<NpgsqlDbContext> options) 
            : base(options)
        {
        }
    }
}

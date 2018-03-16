using System;
using System.Numerics;

namespace Ribosoft.Blast
{
    public class Database
    {
        public string AbsolutePath { get; set; }
        public string RelativePath { get; set; }
        public string AccessionId { get; set; }
        public string AssemblyName { get; set; }
        public int TaxonomyId { get; set; }
        public int SpeciesTaxonomyId { get; set; }
        public string OrganismName { get; set; }
        public string Type { get; set; }
        public BigInteger Nucleotides { get; set; }
        public BigInteger Sequences { get; set; }
        public BigInteger Bytes { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
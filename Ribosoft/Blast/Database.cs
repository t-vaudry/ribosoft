using System;
using System.Numerics;

namespace Ribosoft.Blast
{
    /*! \class Database
     * \brief Object class for properties of the assembly database
     */
    public class Database
    {
        /*! \property AbsolutePath
         * \brief Absolute path to the BLAST database
         */
        public string AbsolutePath { get; set; }
        
        /*! \property RelativePath
         * \brief Relative path to the BLAST database
         */
        public string RelativePath { get; set; }
        
        /*! \property AccessionId
         * \brief Accession id of database organism
         */
        public string AccessionId { get; set; }
        
        /*! \property AssemblyName
         * \brief Assembly name of the organism database
         */
        public string AssemblyName { get; set; }
        
        /*! \property TaxonomyId
         * \brief Taxonomy id of the organism
         */
        public int TaxonomyId { get; set; }
        
        /*! \property SpeciesTaxonomyId
         * \brief Taxonomy id of the organism species
         */
        public int SpeciesTaxonomyId { get; set; }
        
        /*! \property OrganismName
         * \brief Name of the organism
         */
        public string OrganismName { get; set; }
        
        /*! \property Type
         * \brief Organism database type
         */
        public string Type { get; set; }
        
        /*! \property Nucleotides
         * \brief Nucleotides in the database
         */
        public BigInteger Nucleotides { get; set; }
        
        /*! \property Sequences
         * \brief Sequences in the database
         */
        public BigInteger Sequences { get; set; }
        
        /*! \property Bytes
         * \brief Byte information from the database
         */
        public BigInteger Bytes { get; set; }
        
        /*! \property UpdatedAt
         * \brief Time that the database has been updated at
         */
        public DateTime UpdatedAt { get; set; }
    }
}
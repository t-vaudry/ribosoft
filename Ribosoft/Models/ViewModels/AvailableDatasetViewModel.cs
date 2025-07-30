using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.ViewModels
{
    /*! \class AvailableDatasetViewModel
     * \brief View model for displaying available NCBI datasets
     */
    public class AvailableDatasetViewModel
    {
        /*! \property AccessionId
         * \brief Assembly accession ID
         */
        [Display(Name = "Accession")]
        public string AccessionId { get; set; } = string.Empty;

        /*! \property AssemblyName
         * \brief Assembly name
         */
        [Display(Name = "Assembly Name")]
        public string AssemblyName { get; set; } = string.Empty;

        /*! \property OrganismName
         * \brief Organism name
         */
        [Display(Name = "Organism")]
        public string OrganismName { get; set; } = string.Empty;

        /*! \property TaxonomyId
         * \brief NCBI taxonomy ID
         */
        [Display(Name = "Taxonomy ID")]
        public int TaxonomyId { get; set; }

        /*! \property SpeciesId
         * \brief Species taxonomy ID
         */
        [Display(Name = "Species ID")]
        public int SpeciesId { get; set; }

        /*! \property AssemblyLevel
         * \brief Assembly level (Complete, Chromosome, Scaffold, Contig)
         */
        [Display(Name = "Level")]
        public string AssemblyLevel { get; set; } = string.Empty;

        /*! \property SubmissionDate
         * \brief Submission date
         */
        [Display(Name = "Submitted")]
        public DateTime? SubmissionDate { get; set; }

        /*! \property IsAlreadyDownloaded
         * \brief Whether this dataset is already downloaded
         */
        public bool IsAlreadyDownloaded { get; set; }

        /*! \property IsCurrentlyDownloading
         * \brief Whether this dataset is currently being downloaded
         */
        public bool IsCurrentlyDownloading { get; set; }

        /*! \property EstimatedSize
         * \brief Estimated download size in bytes
         */
        [Display(Name = "Est. Size")]
        public long EstimatedSize { get; set; }

        /*! \property ContigCount
         * \brief Number of contigs
         */
        [Display(Name = "Contigs")]
        public int ContigCount { get; set; }

        /*! \property TotalLength
         * \brief Total sequence length
         */
        [Display(Name = "Total Length")]
        public long TotalLength { get; set; }
    }
}

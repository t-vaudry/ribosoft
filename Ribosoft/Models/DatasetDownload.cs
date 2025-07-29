using System;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models
{
    /*! \class DatasetDownload
     * \brief Model for tracking NCBI dataset downloads
     */
    public class DatasetDownload : BaseEntity
    {
        /*! \property Id
         * \brief Primary key
         */
        [Key]
        public int Id { get; set; }

        /*! \property AccessionId
         * \brief Assembly accession ID
         */
        [Required]
        [Display(Name = "Accession ID")]
        public string AccessionId { get; set; } = string.Empty;

        /*! \property AssemblyName
         * \brief Assembly name
         */
        [Required]
        [Display(Name = "Assembly Name")]
        public string AssemblyName { get; set; } = string.Empty;

        /*! \property OrganismName
         * \brief Organism name
         */
        [Required]
        [Display(Name = "Organism")]
        public string OrganismName { get; set; } = string.Empty;

        /*! \property TaxonomyId
         * \brief NCBI taxonomy ID
         */
        [Required]
        [Display(Name = "Taxonomy ID")]
        public int TaxonomyId { get; set; }

        /*! \property SpeciesId
         * \brief Species taxonomy ID
         */
        [Required]
        [Display(Name = "Species ID")]
        public int SpeciesId { get; set; }

        /*! \property Status
         * \brief Download status
         */
        [Required]
        [Display(Name = "Status")]
        public DatasetDownloadStatus Status { get; set; }

        /*! \property Progress
         * \brief Download progress percentage (0-100)
         */
        [Display(Name = "Progress")]
        public int Progress { get; set; }

        /*! \property DownloadUrl
         * \brief NCBI download URL
         */
        public string DownloadUrl { get; set; } = string.Empty;

        /*! \property LocalPath
         * \brief Local file path after download
         */
        public string LocalPath { get; set; } = string.Empty;

        /*! \property FileSize
         * \brief File size in bytes
         */
        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        /*! \property DownloadedBytes
         * \brief Bytes downloaded so far
         */
        public long DownloadedBytes { get; set; }

        /*! \property ErrorMessage
         * \brief Error message if download failed
         */
        public string ErrorMessage { get; set; } = string.Empty;

        /*! \property StartedAt
         * \brief When download started
         */
        [Display(Name = "Started")]
        public DateTime? StartedAt { get; set; }

        /*! \property CompletedAt
         * \brief When download completed
         */
        [Display(Name = "Completed")]
        public DateTime? CompletedAt { get; set; }

        /*! \property IncludeAnnotations
         * \brief Whether to include annotations
         */
        [Display(Name = "Include Annotations")]
        public bool IncludeAnnotations { get; set; }

        /*! \property IncludeSequence
         * \brief Whether to include sequence data
         */
        [Display(Name = "Include Sequence")]
        public bool IncludeSequence { get; set; } = true;

        /*! \property RequestedBy
         * \brief User who requested the download
         */
        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; } = string.Empty;

        /*! \property JobId
         * \brief Hangfire job ID for tracking
         */
        public string JobId { get; set; } = string.Empty;
    }

    /*! \enum DatasetDownloadStatus
     * \brief Status of dataset download
     */
    public enum DatasetDownloadStatus
    {
        /*! \brief Download is queued */
        Queued,
        /*! \brief Download is in progress */
        Downloading,
        /*! \brief Download completed successfully */
        Completed,
        /*! \brief Download failed */
        Failed,
        /*! \brief Download was cancelled */
        Cancelled,
        /*! \brief Processing downloaded file */
        Processing,
        /*! \brief Ready for BLAST database creation */
        ReadyForBlast
    }
}

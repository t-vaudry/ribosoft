using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.ViewModels
{
    /*! \class DatasetDownloadRequestViewModel
     * \brief View model for requesting dataset downloads
     */
    public class DatasetDownloadRequestViewModel
    {
        /*! \property AccessionIds
         * \brief List of accession IDs to download
         */
        [Required]
        [Display(Name = "Selected Datasets")]
        public List<string> AccessionIds { get; set; } = new List<string>();

        /*! \property IncludeAnnotations
         * \brief Whether to include annotation files
         */
        [Display(Name = "Include Annotations")]
        public bool IncludeAnnotations { get; set; } = true;

        /*! \property IncludeSequence
         * \brief Whether to include sequence data
         */
        [Display(Name = "Include Sequence Data")]
        public bool IncludeSequence { get; set; } = true;

        /*! \property CreateBlastDatabase
         * \brief Whether to automatically create BLAST database after download
         */
        [Display(Name = "Create BLAST Database")]
        public bool CreateBlastDatabase { get; set; } = true;

        /*! \property Priority
         * \brief Download priority
         */
        [Display(Name = "Priority")]
        public DownloadPriority Priority { get; set; } = DownloadPriority.Normal;
    }

    /*! \enum DownloadPriority
     * \brief Priority levels for downloads
     */
    public enum DownloadPriority
    {
        /*! \brief Low priority */
        Low,
        /*! \brief Normal priority */
        Normal,
        /*! \brief High priority */
        High
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ribosoft.Models
{
    /*! \class OneTimeCode
     * \brief Model class for one-time authentication codes sent via email
     */
    public class OneTimeCode
    {
        /*! \property Id
         * \brief Primary key
         */
        [Key]
        public int Id { get; set; }

        /*! \property UserId
         * \brief User ID this code belongs to
         */
        [Required]
        public string UserId { get; set; } = string.Empty;

        /*! \property Code
         * \brief The 6-digit one-time code
         */
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        /*! \property Email
         * \brief Email address the code was sent to
         */
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /*! \property CreatedAt
         * \brief When the code was created
         */
        [Required]
        public DateTime CreatedAt { get; set; }

        /*! \property ExpiresAt
         * \brief When the code expires (10 minutes from creation)
         */
        [Required]
        public DateTime ExpiresAt { get; set; }

        /*! \property IsUsed
         * \brief Whether the code has been used
         */
        public bool IsUsed { get; set; } = false;

        /*! \property UsedAt
         * \brief When the code was used (if applicable)
         */
        public DateTime? UsedAt { get; set; }

        /*! \property Purpose
         * \brief Purpose of the code (e.g., "2FA_BYPASS")
         */
        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = string.Empty;

        /*! \property User
         * \brief Navigation property to the user
         */
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        /*! \method IsValid
         * \brief Check if the code is still valid (not expired and not used)
         * \return True if valid, false otherwise
         */
        public bool IsValid()
        {
            return !IsUsed && DateTime.UtcNow <= ExpiresAt;
        }

        /*! \method MarkAsUsed
         * \brief Mark the code as used
         */
        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
        }
    }
}

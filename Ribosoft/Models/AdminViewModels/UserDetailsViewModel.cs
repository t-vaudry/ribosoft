using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ribosoft.Models.AdminViewModels
{
    /*! \class UserDetailsViewModel
     * \brief View model for detailed user information
     */
    public class UserDetailsViewModel
    {
        /*! \property Id
         * \brief User ID
         */
        public string Id { get; set; }

        /*! \property UserName
         * \brief Username
         */
        public string UserName { get; set; }

        /*! \property Email
         * \brief Email address
         */
        public string Email { get; set; }

        /*! \property FirstName
         * \brief First name
         */
        public string FirstName { get; set; }

        /*! \property LastName
         * \brief Last name
         */
        public string LastName { get; set; }

        /*! \property EmailConfirmed
         * \brief Whether email is confirmed
         */
        public bool EmailConfirmed { get; set; }

        /*! \property PhoneNumber
         * \brief Phone number
         */
        public string PhoneNumber { get; set; }

        /*! \property PhoneNumberConfirmed
         * \brief Whether phone number is confirmed
         */
        public bool PhoneNumberConfirmed { get; set; }

        /*! \property TwoFactorEnabled
         * \brief Whether 2FA is enabled
         */
        public bool TwoFactorEnabled { get; set; }

        /*! \property LockoutEnd
         * \brief Lockout end date
         */
        public DateTimeOffset? LockoutEnd { get; set; }

        /*! \property LockoutEnabled
         * \brief Whether lockout is enabled
         */
        public bool LockoutEnabled { get; set; }

        /*! \property AccessFailedCount
         * \brief Number of failed access attempts
         */
        public int AccessFailedCount { get; set; }

        /*! \property Roles
         * \brief User roles
         */
        public List<string> Roles { get; set; } = new List<string>();

        /*! \property JobCount
         * \brief Number of jobs created by user
         */
        public int JobCount { get; set; }

        /*! \property LastLoginDate
         * \brief Last login date
         */
        public DateTime? LastLoginDate { get; set; }

        /*! \property RegistrationDate
         * \brief Registration date
         */
        public DateTime RegistrationDate { get; set; }

        /*! \property RecentJobs
         * \brief Recent jobs created by user
         */
        public object RecentJobs { get; set; }

        /*! \property IsLockedOut
         * \brief Whether user is currently locked out
         */
        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;

        /*! \property DisplayName
         * \brief Display name for the user
         */
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    return $"{FirstName} {LastName}";
                }
                return UserName;
            }
        }
    }

    /*! \class UpdateUserRolesViewModel
     * \brief View model for updating user roles
     */
    public class UpdateUserRolesViewModel
    {
        /*! \property UserId
         * \brief User ID
         */
        [Required]
        public string UserId { get; set; }

        /*! \property Roles
         * \brief List of roles to assign
         */
        public List<string> Roles { get; set; } = new List<string>();
    }
}

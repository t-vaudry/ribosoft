using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Ribosoft.Models.AdminViewModels
{
    /*! \class UserManagementViewModel
     * \brief View model for the admin user management page
     */
    public class UserManagementViewModel
    {
        /*! \property Users
         * \brief List of users to display
         */
        public List<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();

        /*! \property SearchTerm
         * \brief Current search term
         */
        public string SearchTerm { get; set; } = "";

        /*! \property RoleFilter
         * \brief Current role filter
         */
        public string RoleFilter { get; set; } = "";

        /*! \property CurrentPage
         * \brief Current page number
         */
        public int CurrentPage { get; set; } = 1;

        /*! \property PageSize
         * \brief Number of items per page
         */
        public int PageSize { get; set; } = 20;

        /*! \property TotalUsers
         * \brief Total number of users
         */
        public int TotalUsers { get; set; }

        /*! \property TotalPages
         * \brief Total number of pages
         */
        public int TotalPages { get; set; }

        /*! \property AvailableRoles
         * \brief List of available roles for filtering
         */
        public List<string> AvailableRoles { get; set; } = new List<string>();

        /*! \property HasPreviousPage
         * \brief Whether there is a previous page
         */
        public bool HasPreviousPage => CurrentPage > 1;

        /*! \property HasNextPage
         * \brief Whether there is a next page
         */
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    /*! \class UserListItemViewModel
     * \brief View model for individual user items in the list
     */
    public class UserListItemViewModel
    {
        /*! \property Id
         * \brief User ID
         */
        public string Id { get; set; } = "";

        /*! \property UserName
         * \brief Username
         */
        public string UserName { get; set; } = "";

        /*! \property Email
         * \brief Email address
         */
        public string Email { get; set; } = "";

        /*! \property FirstName
         * \brief First name
         */
        public string? FirstName { get; set; }

        /*! \property LastName
         * \brief Last name
         */
        public string? LastName { get; set; }

        /*! \property EmailConfirmed
         * \brief Whether email is confirmed
         */
        public bool EmailConfirmed { get; set; }

        /*! \property LockoutEnd
         * \brief Lockout end date
         */
        public DateTimeOffset? LockoutEnd { get; set; }

        /*! \property AccessFailedCount
         * \brief Number of failed access attempts
         */
        public int AccessFailedCount { get; set; }

        /*! \property TwoFactorEnabled
         * \brief Whether 2FA is enabled
         */
        public bool TwoFactorEnabled { get; set; }

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

        /*! \property PrimaryRole
         * \brief Primary role for display
         */
        public string PrimaryRole
        {
            get
            {
                if (Roles.Contains("Administrator"))
                    return "Administrator";
                if (Roles.Contains("User"))
                    return "User";
                if (Roles.Contains("Guest"))
                    return "Guest";
                return Roles.FirstOrDefault() ?? "No Role";
            }
        }
    }
}

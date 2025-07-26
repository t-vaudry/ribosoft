using System.Collections.Generic;

namespace Ribosoft.Models.AdminViewModels
{
    /*! \class EditUserRolesViewModel
     * \brief View model for edit user roles modal
     */
    public class EditUserRolesViewModel
    {
        /*! \property UserId
         * \brief User ID
         */
        public string UserId { get; set; } = string.Empty;

        /*! \property UserName
         * \brief Username
         */
        public string UserName { get; set; } = string.Empty;

        /*! \property DisplayName
         * \brief Display name
         */
        public string DisplayName { get; set; } = string.Empty;

        /*! \property AvailableRoles
         * \brief List of all available roles
         */
        public List<string> AvailableRoles { get; set; } = new List<string>();

        /*! \property UserRoles
         * \brief Current user roles
         */
        public List<string> UserRoles { get; set; } = new List<string>();

        /*! \property SelectedRoles
         * \brief Selected roles for update
         */
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}

namespace Ribosoft.Models.RequestViewModels
{
	/*! \class TargetRegionViewModel
     * \brief Model class for the target region view
     */
    public class TargetRegionViewModel
    {
    	/*! \property Id
         * \brief Target region Id
         */
        public int Id { get; set; }

        /*! \property Name
         * \brief Target region name
         */
        public string Name { get; set; } = string.Empty;

        /*! \property Selected
         * \brief Check if selected
         */
        public bool Selected { get; set; }
    }
}

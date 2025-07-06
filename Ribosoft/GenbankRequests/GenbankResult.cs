namespace Ribosoft.GenbankRequests
{
	/*! \class GenbankResult
     * \brief Object class for the result from a GenBank request
     */
    public class GenbankResult
    {
        /*! \property AccessionId
         * \brief Accession ID of the RNA sequence
         */
        public string AccessionId { get; set; } = string.Empty;

        /*! \property Sequence
         * \brief RNA sequence associated to accession number
         */
        public string Sequence { get; set; } = string.Empty;

        /*! \property OpenReadingFrameStart
         * \brief Index for the start of the open reading frame
         */
        public int OpenReadingFrameStart { get; set; }

        /*! \property OpenReadingFrameEnd
         * \brief Index for the end of the open reading frame
         */
        public int OpenReadingFrameEnd { get; set; }
    }
}
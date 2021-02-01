using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//! \namespace Ribosoft.Biology
namespace Ribosoft.Biology
{
    /*! \class SubstrateInfo
     * \brief Class that contains the information of the substrate
     */
    public class SubstrateInfo
    {
        /*! \property Sequence
         * \brief Sequence of the substrate
         */
        public Sequence Sequence { get; set; }

        /*! \property Structure
         * \brief Structure of the substrate
         */
        public String Structure { get; set; }

        /*! \property CutsiteOffset
         * \brief Cutsite offset of the substrate
         */
        public int CutsiteOffset { get; set; }

        /*!
         * \brief Default constructor
         */
        public SubstrateInfo()
        {
            Sequence = null;
            Structure = null;
            CutsiteOffset = 0;
        }

        /*!
         * \brief Constructor
         * \param seq Substrate sequence
         * \param struc Substrate structure
         * \param offset Substrate cutsite offset (default=0)
         */
        public SubstrateInfo(Sequence seq, String struc, int offset = 0)
        {
            Sequence = seq;
            Structure = struc;
            CutsiteOffset = offset;
        }
    }
}

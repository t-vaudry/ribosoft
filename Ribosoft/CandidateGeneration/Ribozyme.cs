using System;
using System.Collections.Generic;
using System.Text;

namespace Ribosoft.CandidateGeneration
{
    /*! \class Ribozyme
     * \brief Object class for the ribozyme properties used in candidate generation
     */
    public class Ribozyme
    {
        /*! \property Sequence
         * \brief Sequence of the ribozyme
         */
        public String Sequence { get; set; }

        /*! \property Structure
         * \brief Structure of the ribozyme
         */
        public String Structure { get; set; }

        /*! \property SubstrateSequence
         * \brief Substrate sequence for the ribozyme template
         */
        public String SubstrateSequence { get; set; }

        /*! \property SubstrateStructure
         * \brief Substrate structure for the ribozyme template
         */
        public String SubstrateStructure { get; set; }

        /*!
         * \brief Default constructor
         */
        public Ribozyme()
        {
        }

        /*!
         * \brief Constructor
         * \param seq Ribozyme template sequence
         * \param struc Ribozyme template structure
         * \param cutSite Ribozyme template substrate sequence
         * \param cutSiteStruc Ribozyme template substrate structure
         */
        public Ribozyme(String seq, String struc, String cutSite, String cutSiteStruc)
        {
            Sequence = seq;
            Structure = struc;
            SubstrateSequence = cutSite;
            SubstrateStructure = cutSiteStruc;
        }
    }
}

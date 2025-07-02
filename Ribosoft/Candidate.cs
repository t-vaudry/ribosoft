using System;
using System.Collections.Generic;
using Ribosoft.Biology;
using Ribosoft.MultiObjectiveOptimization;

namespace Ribosoft 
{
    /*! \class Candidate
     * \brief Object class for the candidates evaluated during candidate generation
     */
    public class Candidate
    {
        /*! \property Sequence 
         * \brief Ribozyme sequence of the candidate
         */
        public Sequence? Sequence { get; set; }

        /*! \property Structure
         * \brief Ribozyme structure of the candidate
         */
        public String? Structure { get; set; }

        /*! \property SubstrateSequence
         * \brief Substrate sequence of the candidate
         */
        public String? SubstrateSequence { get; set; }

        /*! \property SubstrateStructure
         * \brief Substrate structure of the candidate
         */
        public String? SubstrateStructure { get; set; }

        /*! \property CutsiteNumberOffset
         * \brief Initial cutsite number offset
         */
        public int CutsiteNumberOffset { get; set; }

        /*! \property CutsiteIndices
         * \brief List of cutsides indices of the candidate
         */
        public List<int>? CutsiteIndices { get; set; }

        /*!
         * \brief Default constructor
         */
        public Candidate()
        {
            CutsiteNumberOffset = 0;
        }
    }
}
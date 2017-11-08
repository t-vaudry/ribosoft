using System;
using System.Collections.Generic;
using System.Text;

namespace TestCandidateGeneration
{
    class Ribozyme
    {
        public String mSequence;
        public String mStructure;
        public String mCutSite;
        public List<int> mRNALinkIndices;

        public Ribozyme()
        {
            mRNALinkIndices = new List<int>();
        }

        public Ribozyme(String seq, String struc, String cutSite)
        {
            mSequence = seq;
            mStructure = struc;
            mCutSite = cutSite;
            mRNALinkIndices = new List<int>();
        }
    }
}

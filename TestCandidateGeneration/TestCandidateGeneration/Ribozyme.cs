using System;
using System.Collections.Generic;
using System.Text;

namespace TestCandidateGeneration
{
    class Ribozyme
    {
        public String mSequence;
        public String mStructure;
        public String mSubstrateSequence;
        public String mSubstrateStructure;

        public Ribozyme()
        {
        }

        public Ribozyme(String seq, String struc, String cutSite, String cutSiteStruc)
        {
            mSequence = seq;
            mStructure = struc;
            mSubstrateSequence = cutSite;
            mSubstrateStructure = cutSiteStruc;
        }
    }
}

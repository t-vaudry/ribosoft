using System;
using System.Collections.Generic;
using System.Text;

namespace Ribosoft.CandidateGeneration
{
    public class Ribozyme
    {
        public String Sequence { get; set; }
        public String Structure { get; set; }
        public String SubstrateSequence { get; set; }
        public String SubstrateStructure { get; set; }

        public Ribozyme()
        {
        }

        public Ribozyme(String seq, String struc, String cutSite, String cutSiteStruc)
        {
            Sequence = seq;
            Structure = struc;
            SubstrateSequence = cutSite;
            SubstrateStructure = cutSiteStruc;
        }
    }
}

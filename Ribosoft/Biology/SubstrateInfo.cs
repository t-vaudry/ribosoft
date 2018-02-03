using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.Biology
{
    public class SubstrateInfo
    {
        public Sequence Sequence { get; set; }
        public String Structure { get; set; }
        public int CutsiteOffset { get; set; }

        public SubstrateInfo()
        {
            Sequence = null;
            Structure = null;
            CutsiteOffset = 0;
        }

        public SubstrateInfo(Sequence seq, String struc, int offset = 0)
        {
            Sequence = seq;
            Structure = struc;
            CutsiteOffset = offset;
        }
    }
}

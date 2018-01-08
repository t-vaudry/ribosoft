using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribosoft.CandidateGeneration
{
    public class CandidateGenerationResult
    {
        public R_STATUS Status { get; set; }
        public List<Sequence> RibozymCandidates { get; set; }

        public CandidateGenerationResult()
        {
            Status = R_STATUS.R_SYSTEM_ERROR_FIRST;
            RibozymCandidates = new List<Sequence>();
        }
    }
}

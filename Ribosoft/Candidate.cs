using System;
using System.Collections.Generic;

namespace Ribosoft {

    public class Candidate
    {
        public Sequence Sequence { get; set; }
        public List<int> CutsiteIndices { get; set; }
        public float[] FitnessValues { get; set; }
        public bool IsRanked { get; set; }
        public int Rank { get; set; }

        public Candidate()
        {
            Sequence = null;
            CutsiteIndices = null;
            FitnessValues = new float[4];
            IsRanked = false;
            Rank = -1;
        }
    }
}
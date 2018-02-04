using System;
using System.Collections.Generic;
using Ribosoft.Biology;
using Ribosoft.MultiObjectiveOptimization;

namespace Ribosoft {

    public class Candidate : IRankable<float>
    {
        public Sequence Sequence { get; set; }
        public String Structure { get; set; }

        public String SubstrateSequence { get; set; }
        public String SubstrateStructure { get; set; }
        public int CutsiteNumberOffset { get; set; }
        public List<int> CutsiteIndices { get; set; }
        public float[] FitnessValues { get; set; }
        public int Rank { get; set; }

        public Candidate()
        {
            Sequence = null;
            Structure = null;
            SubstrateSequence = null;
            CutsiteNumberOffset = 0;
            CutsiteIndices = null;
            FitnessValues = new float[4];
            Rank = 0;
        }

        public IEnumerable<float> Comparables => FitnessValues;
    }
}
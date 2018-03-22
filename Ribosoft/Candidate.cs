using System;
using System.Collections.Generic;
using Ribosoft.Biology;
using Ribosoft.MultiObjectiveOptimization;

namespace Ribosoft {

    public class Candidate : IRankable<OptimizeItem<float>>
    {
        public Sequence Sequence { get; set; }
        public String Structure { get; set; }

        public String SubstrateSequence { get; set; }
        public String SubstrateStructure { get; set; }
        public int CutsiteNumberOffset { get; set; }
        public List<int> CutsiteIndices { get; set; }
        public OptimizeItem<float>[] FitnessValues { get; set; }
        public int Rank { get; set; }

        public Candidate()
        {
            Sequence = null;
            Structure = null;
            SubstrateSequence = null;
            CutsiteNumberOffset = 0;
            CutsiteIndices = null;
            FitnessValues = new OptimizeItem<float>[4];
            Rank = 0;
        }

        public IEnumerable<OptimizeItem<float>> Comparables => FitnessValues;
    }
}
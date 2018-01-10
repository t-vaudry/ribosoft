using System;

namespace Ribosoft.MultiObjectiveOptimization {

    public class Candidate
    {
        public float[] FitnessValues { get; set; }
        public int Rank { get; set; }

        public Candidate()
        {
            FitnessValues = null;
            Rank = -1;
        }

        public Candidate(float[] values)
        {
            FitnessValues = values;
            Rank = -1;
        }
    }
}
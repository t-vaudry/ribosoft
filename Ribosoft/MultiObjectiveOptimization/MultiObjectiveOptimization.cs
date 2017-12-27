using System;
using System.Collections.Generic;
using System.Linq;

namespace Ribosoft.MultiObjectiveOptimization {

    public class MultiObjectiveOptimizer
    {
        public const float Tolerance = 0.05f;
        public List<Candidate> Candidates { get; set; }

        public MultiObjectiveOptimizer()
        {
            Candidates = null;
        }

        public void Optimize(int Rank)
        {
            List<Candidate> FrontCandidates = new List<Candidate>();

            foreach (Candidate Victim in Candidates) {
                bool Dominated = false;

                foreach (Candidate Dominator in Candidates) {
                    if (Dominator == Victim) {
                        continue;
                    }

                    if (ParetoDominate(Victim, Dominator)) {
                        Dominated = true;
                        break;
                    }
                }

                if (!Dominated) {
                    FrontCandidates.Add(Victim);
                }
            }

            // TODO: Remove, I don't think this is necessary anymore?
            if (FrontCandidates.Count != 0) {
                foreach (Candidate RankedCandidate in FrontCandidates) {
                    RankedCandidate.Rank = Rank;
                    Candidates.Remove(RankedCandidate);
                }
            } else {
                foreach (Candidate Candidate in Candidates) {
                    Candidate.Rank = Rank;
                }

                Candidates.Clear();
            }

            if (Candidates.Count != 0) {
                Rank++;
                Optimize(Rank);
            }
        }

        private bool ParetoDominate(Candidate Victim, Candidate Dominator)
        {
            bool Dominated = true;
            bool StrictlyDominated = false;
            var Properties = Victim.FitnessValues.Zip(Dominator.FitnessValues, (x, y) => new { Victim = x, Dominator = y });
            foreach (var Property in Properties) {
                if (Property.Dominator <= Property.Victim) {
                    if (Math.Abs(Property.Dominator - Property.Victim) > Tolerance) {
                        StrictlyDominated = true;
                    }
                } else {
                    Dominated = false;
                    break;
                }
            }

            return Dominated && StrictlyDominated;
        }
    }
}
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

        /* Optimization using Pareto Ranking
        ** Recursive implementation where candidates are compared
        ** to find dominated candidates. Those who are not dominated
        ** are ranked, and removed from the list. This happens
        ** recursively until there are no more candidates to rank.
        */
        public void Optimize(int Rank)
        {
            // List for the current rank
            List<Candidate> FrontCandidates = new List<Candidate>();

            foreach (Candidate Victim in Candidates) {
                bool Dominated = false;

                foreach (Candidate Dominator in Candidates) {
                    // If the same candidate, continue
                    if (Dominator == Victim) {
                        continue;
                    }

                    // Check for dominance, and break if candidate is dominated
                    try {
                        if (ParetoDominate(Victim, Dominator))
                        {
                            Dominated = true;
                            break;
                        }
                    } catch (MultiObjectiveOptimizationException e) {
                        throw e;
                    }
                }

                // If not dominated by any other candidate, add to Pareto Front
                if (!Dominated) {
                    FrontCandidates.Add(Victim);
                }
            }

            // Rank non-dominated candidates, and remove from list
            // If all candidates are dominated, rank them all equally
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

            // Recursively call function to continue ranking
            if (Candidates.Count != 0) {
                Rank++;
                Optimize(Rank);
            }
        }

        /* Pareto Dominance
        ** A vector x dominates y:
        **   1. If fi(x) <= fi(y) for all i functions of f, and
        **   2. There is at least one i such that fi(x) < fi(y)
        */
        private bool ParetoDominate(Candidate Victim, Candidate Dominator)
        {
            bool Dominated = true;
            bool StrictlyDominated = false;

            // If two candidates have different number of fitness values, something went wrong
            if (Victim.FitnessValues.Length != Dominator.FitnessValues.Length) {
                throw new MultiObjectiveOptimizationException(R_STATUS.R_FITNESS_VALUE_LENGTHS_DIFFER, "Candidates have different number of fitness values!");
            }

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
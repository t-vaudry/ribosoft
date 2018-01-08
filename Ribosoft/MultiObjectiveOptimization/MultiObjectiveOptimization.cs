using System;
using System.Collections.Generic;
using System.Linq;

namespace Ribosoft.MultiObjectiveOptimization {

    public class MultiObjectiveOptimizer
    {
        public const float Tolerance = 0.05f;

        public MultiObjectiveOptimizer()
        {
        }

        /* Optimization using Pareto Ranking
        ** Recursive implementation where candidates are compared
        ** to find dominated candidates. Those who are not dominated
        ** are ranked, and removed from the list. This happens
        ** recursively until there are no more candidates to rank.
        */
        public void Optimize(List<Candidate> candidates, int rank)
        {
            // If candidates are empty, return
            if (candidates.Count == 0) {
                throw new MultiObjectiveOptimizationException(R_STATUS.R_EMPTY_CANDIDATE_LIST, "List of Candidates is empty!");
            }

            // List for the current rank
            List<Candidate> frontCandidates = new List<Candidate>();

            foreach (Candidate victim in candidates) {
                bool dominated = false;

                foreach (Candidate dominator in candidates.Where(candidate => candidate != victim)) {
                    // Check for dominance, and break if candidate is dominated
                    if (ParetoDominate(victim, dominator)) {
                        dominated = true;
                        break;
                    }
                }

                // If not dominated by any other candidate, add to Pareto Front
                if (!dominated) {
                    frontCandidates.Add(victim);
                }
            }

            // Rank non-dominated candidates, and remove from list
            // If all candidates are dominated, rank them all equally
            if (frontCandidates.Count != 0) {
                foreach (Candidate rankedCandidate in frontCandidates) {
                    rankedCandidate.Rank = rank;
                    candidates.Remove(rankedCandidate);
                }
            } else {
                foreach (Candidate candidate in candidates) {
                    candidate.Rank = rank;
                }

                candidates.Clear();
            }

            // Recursively call function to continue ranking
            if (candidates.Count != 0) {
                rank++;
                Optimize(candidates, rank);
            }
        }

        /* Pareto Dominance
        ** A vector x dominates y:
        **   1. If fi(x) <= fi(y) for all i functions of f, and
        **   2. There is at least one i such that fi(x) < fi(y)
        */
        private bool ParetoDominate(Candidate victim, Candidate dominator)
        {
            bool dominated = true;
            bool strictlyDominated = false;

            // If two candidates have different number of fitness values, something went wrong
            if (victim.FitnessValues.Length != dominator.FitnessValues.Length) {
                throw new MultiObjectiveOptimizationException(R_STATUS.R_FITNESS_VALUE_LENGTHS_DIFFER, "Candidates have different number of fitness values!");
            }

            // Merge fitness values for comparison
            var Properties = victim.FitnessValues.Zip(dominator.FitnessValues, (x, y) => new { Victim = x, Dominator = y });
            foreach (var Property in Properties) {
                if (Property.Dominator <= Property.Victim) {
                    if (Math.Abs(Property.Dominator - Property.Victim) > Tolerance) {
                        strictlyDominated = true;
                    }
                } else {
                    dominated = false;
                    break;
                }
            }

            return dominated && strictlyDominated;
        }
    }
}
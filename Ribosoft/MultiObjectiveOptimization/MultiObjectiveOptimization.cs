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
        public IList<T> Optimize<T>(IList<T> candidates, int rank) where T : class, IRankable<OptimizeItem<float>>
        {
            // If candidates are empty, return
            if (candidates.Count == 0) {
                throw new MultiObjectiveOptimizationException(R_STATUS.R_EMPTY_CANDIDATE_LIST, "List of Candidates is empty!");
            }

            List<T> rankedCandidates = new List<T>();

            // List for the current rank
            IList<T> frontCandidates = new List<T>();

            foreach (var victim in candidates) {
                var dominated = candidates.Where(candidate => candidate != victim).Any(dominator => ParetoDominate(victim, dominator));

                // If not dominated by any other candidate, add to Pareto Front
                if (!dominated) {
                    frontCandidates.Add(victim);
                }
            }

            // Rank non-dominated candidates, and remove from list
            // If all candidates are dominated, rank them all equally
            if (frontCandidates.Any()) {
                foreach (var rankedCandidate in frontCandidates) {
                    rankedCandidate.Rank = rank;
                    rankedCandidates.Add(rankedCandidate);
                    candidates.Remove(rankedCandidate);
                }
            } else {
                foreach (var candidate in candidates) {
                    candidate.Rank = rank;
                    rankedCandidates.Add(candidate);
                    candidates.Remove(candidate);
                }
            }

            // Recursively call function to continue ranking
            if (candidates.Any()) {
                rank++;
                rankedCandidates.AddRange(Optimize(candidates, rank));
            }

            return rankedCandidates;
        }

        /* Pareto Dominance
        ** A vector x dominates y:
        **   1. If fi(x) <= fi(y) for all i functions of f, and
        **   2. There is at least one i such that fi(x) < fi(y)
        */
        private bool ParetoDominate(IRankable<OptimizeItem<float>> victim, IRankable<OptimizeItem<float>> dominator)
        {
            bool dominated = true;
            bool strictlyDominated = false;

            using (var dominatorEnumerator = dominator.Comparables.GetEnumerator())
            {
                foreach (var v in victim.Comparables)
                {
                    // If two candidates have different number of fitness values, something went wrong
                    if (!dominatorEnumerator.MoveNext())
                    {
                        throw new MultiObjectiveOptimizationException(R_STATUS.R_FITNESS_VALUE_LENGTHS_DIFFER, "Candidates have different number of fitness values!");
                    }

                    var d = dominatorEnumerator.Current;

                    if (((d.Type == OptimizeType.MIN) && (d.Value <= v.Value)) || (((d.Type == OptimizeType.MAX)) && (d.Value >= v.Value)))
                    {
                        if (Math.Abs(d.Value - v.Value) > Tolerance)
                        {
                            strictlyDominated = true;
                        }
                    }
                    else
                    {
                        dominated = false;
                        break;
                    }
                }
            }

            return dominated && strictlyDominated;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ribosoft.MultiObjectiveOptimization 
{
    /*! \class MultiObjectiveOptimizer
     * \brief Object class for the multi-objective optimization of ribozyme designs
     */
    public class MultiObjectiveOptimizer
    {
        /*! \fn MultiObjectiveOptimizer
         * \brief Default constructor
         */
        public MultiObjectiveOptimizer()
        {
        }

        /*! \fn Optimize<T>
         * \brief Optimization using Pareto Ranking
         * Recursive implementation where candidates are compared to find dominated candidates. Those who are not dominated are ranked, and removed from the list. This happens recursively until there are no more candidates to rank.
         * \param candidates List of candidates
         * \param rank Current rank
         * \return List of ranked candidates
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
            // They will be reranked after using partial dominance
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

            // Try to rerank equally ranked candidates. If a candidate beats another candidate in more fitness categories, it will be ranked higher
            // If candidate A beats candidate B in more categories, candidate B beats candidate C and candidate C beats candidate A, they will be ranked equally
            rankedCandidates = UpdateRank(rankedCandidates, ref rank);

            // Recursively call function to continue ranking
            if (candidates.Any()) {
                rankedCandidates.AddRange(Optimize(candidates, rank));
            }

            return rankedCandidates;
        }

        /*! \fn ParetoDominate
         * \brief Function to determine Pareto Dominance
         * A vector x dominates y:
         *   1. If fi(x) <= fi(y) for all i functions of f, and
         *   2. There is at least one i such that fi(x) < fi(y)
         * \param victim Victim optimize item
         * \param dominator Dominator optimize item
         * \return Boolean result of dominance check
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
                        if (Math.Abs(d.Value - v.Value) > v.Tolerance)
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

        /*! \fn UpdateRank<T>
         * \brief Optimization using Partial Ranking
         * Recursive implementation where candidates are compared to find partially dominating candidates. Those who are not partially dominated are ranked, and removed from the list. This happens recursively until there are no more candidates to rank.
         * \param candidates List of candidates
         * \param rank Current rank passed by reference
         * \return List of ranked candidates
         */
        public List<T> UpdateRank<T>(List<T> candidates, ref int rank) where T : class, IRankable<OptimizeItem<float>>
        {
            List<T> rankedCandidates = new List<T>();

            // List for the current rank
            List<T> frontCandidates = new List<T>();

            foreach (var victim in candidates)
            {
                var partiallyDominant = candidates.Where(candidate => candidate != victim).Any(dominator => PartialDominate(victim, dominator));

                // If not dominated by any other candidate, add to Partial Dominance Front
                if (partiallyDominant)
                {
                    frontCandidates.Add(victim);
                }
            }

            // Rank non-dominated candidates, and remove from list
            // If all candidates are dominated, rank them all equally
            if (frontCandidates.Any())
            {
                // If the number of initial candidates and the number of front candidates are the same, we have a circular "rank" (A beats B beats C beats A)
                // Assign them the same rank
                if (candidates.Count() == frontCandidates.Count())
                {
                    foreach (var rankedCandidate in frontCandidates)
                    {
                        rankedCandidate.Rank = rank;
                        rankedCandidates.Add(rankedCandidate);
                    }
                    rank++;
                }
                else
                {
                    foreach (var rankedCandidate in frontCandidates)
                    {
                        candidates.Remove(rankedCandidate);
                    }

                    if (frontCandidates.Count() == 1)
                    {
                        // If only one candidate is in the front, skip the function call and simply assign it the current rank. 
                        frontCandidates[0].Rank = rank;
                        rankedCandidates.Add(frontCandidates[0]);
                        rank++;
                    }
                    else
                    {
                        // Recursively call function to continue ranking
                        rankedCandidates.AddRange(UpdateRank(frontCandidates, ref rank));
                    }

                    if (candidates.Any())
                    {
                        // If only one candidate is left, skip the function call and simply assign it the current rank. 
                        if (candidates.Count() == 1)
                        {
                            candidates[0].Rank = rank;
                            rankedCandidates.Add(candidates[0]);
                            rank++;
                        }
                        else
                        {
                            // Recursively call function to continue ranking
                            rankedCandidates.AddRange(UpdateRank(candidates, ref rank));
                        }
                    }
                }
            }
            else
            {
                foreach (var candidate in candidates)
                {
                    candidate.Rank = rank;
                    rankedCandidates.Add(candidate);
                }
                rank++;
            }

            return rankedCandidates;
        }

        /*! \fn PartialDominate
         * \brief Function to determine Partial Dominance
         * A vector x partially dominates y:
         *   1. If (fi(x) < fi(y)) > (fi(x) > fi(y)) for all i functions of f
         * \param victim Victim optimize item
         * \param dominator Dominator optimize item
         * \return Boolean result of partial dominance check
         */
        private bool PartialDominate(IRankable<OptimizeItem<float>> victim, IRankable<OptimizeItem<float>> dominator)
        {
            int victimScore = 0;
            int dominatorScore = 0;

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

                    if (d.Value == v.Value)
                        continue;

                    if (((d.Type == OptimizeType.MIN) && (d.Value < v.Value)) || (((d.Type == OptimizeType.MAX)) && (d.Value > v.Value)))
                    {
                        dominatorScore++;
                    }
                    else
                    {
                        victimScore++;
                    }
                }
            }

            return (victimScore > dominatorScore);
        }
    }
}
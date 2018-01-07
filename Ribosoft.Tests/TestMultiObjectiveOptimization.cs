using System;
using System.Collections.Generic;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestMultiObjectiveOptimization
    {
        [Fact]
        public void EqualInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 1.0f, 1.0f };
            float[] second = { 1.0f, 1.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
        }

        [Fact]
        public void NearlyEqualInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 1.0f, 1.0f };
            float[] second = { 1.05f, 1.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
        }

        [Fact]
        public void Valid2DInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 1.0f, 1.0f };
            float[] second = { 2.0f, 2.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void Valid3DInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 1.0f, 1.0f, 1.0f };
            float[] second = { 2.0f, 2.0f, 2.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void Valid4DInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] second = { 2.0f, 2.0f, 2.0f, 2.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void InvalidFitnessValues()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 2.0f, 1.0f, 1.0f };
            float[] second = { 2.0f, 2.0f, 2.0f };
            float[] third = { 3.0f, 2.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);
            MultiObjectiveOptimization.Candidate three = new MultiObjectiveOptimization.Candidate(third);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);
            candidates.Add(three);

            try {
                multiObjectiveOptimizer.Optimize(candidates, 1);
            } catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException Exception) {
                Assert.Equal(Exception.Code, R_STATUS.R_FITNESS_VALUE_LENGTHS_DIFFER);
                Assert.Equal(Exception.Message, "Candidates have different number of fitness values!");
            }
        }

        [Fact]
        public void InvalidCandidateList()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            try {
                multiObjectiveOptimizer.Optimize(new List<MultiObjectiveOptimization.Candidate>(), 1);
            } catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException Exception) {
                Assert.Equal(Exception.Code, R_STATUS.R_EMPTY_CANDIDATE_LIST);
                Assert.Equal(Exception.Message, "List of Candidates is empty!");
            }
        }
    }
}
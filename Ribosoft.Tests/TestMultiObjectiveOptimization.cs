using System;
using System.Collections.Generic;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestMultiObjectiveOptimization
    {
        [Fact]
        public void ValidInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 1.0f, 1.0f };
            float[] second = { 2.0f, 2.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);

            multiObjectiveOptimizer.Candidates = candidates;

            multiObjectiveOptimizer.Optimize(1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void AnotherValidInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            float[] first = { 2.0f, 1.0f, 1.0f };
            float[] second = { 2.0f, 2.0f, 2.0f };
            float[] third = { 3.0f, 2.0f, 3.0f };

            MultiObjectiveOptimization.Candidate one = new MultiObjectiveOptimization.Candidate(first);
            MultiObjectiveOptimization.Candidate two = new MultiObjectiveOptimization.Candidate(second);
            MultiObjectiveOptimization.Candidate three = new MultiObjectiveOptimization.Candidate(third);

            List<MultiObjectiveOptimization.Candidate> candidates = new List<MultiObjectiveOptimization.Candidate>();
            candidates.Add(one);
            candidates.Add(two);
            candidates.Add(three);

            multiObjectiveOptimizer.Candidates = candidates;

            multiObjectiveOptimizer.Optimize(1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
            Assert.Equal(3, three.Rank);
        }
    }
}
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

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
        }

        [Fact]
        public void NearlyEqualInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.05f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
        }

        [Fact]
        public void Valid2DInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void Valid3DInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void Valid4DInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void InvalidFitnessValues()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] third = {
                new MultiObjectiveOptimization.OptimizeItem<float>(3.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };
            Candidate three = new Candidate { FitnessValues = third };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two,
                three
            };

            try {
                multiObjectiveOptimizer.Optimize(candidates, 1);
            } catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException Exception) {
                Assert.Equal(R_STATUS.R_FITNESS_VALUE_LENGTHS_DIFFER, Exception.Code);
                Assert.Equal("Candidates have different number of fitness values!", Exception.Message);
            }
        }

        [Fact]
        public void InvalidCandidateList()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            try {
                multiObjectiveOptimizer.Optimize(new List<Candidate>(), 1);
            } catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException Exception) {
                Assert.Equal(R_STATUS.R_EMPTY_CANDIDATE_LIST, Exception.Code);
                Assert.Equal("List of Candidates is empty!", Exception.Message);
            }
        }

        [Fact]
        public void Maximize1D()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            MultiObjectiveOptimization.OptimizeItem<float>[] first = {
                new MultiObjectiveOptimization.OptimizeItem<float>(1.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(3.0f, MultiObjectiveOptimization.OptimizeType.MAX)
            };
            MultiObjectiveOptimization.OptimizeItem<float>[] second = {
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MIN),
                new MultiObjectiveOptimization.OptimizeItem<float>(2.0f, MultiObjectiveOptimization.OptimizeType.MAX)
            };

            Candidate one = new Candidate { FitnessValues = first };
            Candidate two = new Candidate { FitnessValues = second };

            List<Candidate> candidates = new List<Candidate>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(candidates, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }
    }
}
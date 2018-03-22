using System;
using System.Collections.Generic;
using Xunit;
using Ribosoft.Models;

namespace Ribosoft.Tests
{
    public class TestMultiObjectiveOptimization
    {
        [Fact]
        public void EqualInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            Design two = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            List<Design> designs = new List<Design>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(designs, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
        }

        [Fact]
        public void NearlyEqualInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            Design two = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.05f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            List<Design> designs = new List<Design>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(designs, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
        }

        [Fact]
        public void ValidInput()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            Design two = new Design
            {
                AccessibilityScore = 2.0f,
                HighestTemperatureScore = 0.0f,
                DesiredTemperatureScore = 2.0f,
                SpecificityScore = 2.0f,
                StructureScore = 2.0f
            };

            List<Design> designs = new List<Design>
            {
                one,
                two
            };

            multiObjectiveOptimizer.Optimize(designs, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(2, two.Rank);
        }

        [Fact]
        public void InvalidFitnessValues()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            Design two = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f
            };

            Design three = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
            };

            List<Design> designs = new List<Design>
            {
                one,
                two,
                three
            };

            try {
                multiObjectiveOptimizer.Optimize(designs, 1);
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
                multiObjectiveOptimizer.Optimize(new List<Design>(), 1);
            } catch (MultiObjectiveOptimization.MultiObjectiveOptimizationException Exception) {
                Assert.Equal(R_STATUS.R_EMPTY_CANDIDATE_LIST, Exception.Code);
                Assert.Equal("List of Candidates is empty!", Exception.Message);
            }
        }
    }
}
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
                StructureScore = 1.0f,
                Job = new Job {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design two = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
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
        public void PartialRanking()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design
            {
                AccessibilityScore = 3.0f,
                HighestTemperatureScore = 10.0f,
                DesiredTemperatureScore = 2.0f,
                SpecificityScore = 2.0f,
                StructureScore = 3.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design two = new Design
            {
                AccessibilityScore = 2.0f,
                HighestTemperatureScore = 9.0f,
                DesiredTemperatureScore = 3.0f,
                SpecificityScore = 2.0f,
                StructureScore = 2.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design three = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 8.0f,
                DesiredTemperatureScore = 4.0f,
                SpecificityScore = 3.0f,
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design four = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 8.0f,
                DesiredTemperatureScore = 4.0f,
                SpecificityScore = 3.0f,
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design five = new Design
            {
                AccessibilityScore = 7.0f,
                HighestTemperatureScore = 4.0f,
                DesiredTemperatureScore = 7.0f,
                SpecificityScore = 1.0f,
                StructureScore = 7.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            List<Design> designs = new List<Design>
            {
                one,
                two,
                three,
                four,
                five
            };

            multiObjectiveOptimizer.Optimize(designs, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
            Assert.Equal(2, three.Rank);
            Assert.Equal(2, four.Rank);
            Assert.Equal(3, five.Rank);
        }

        [Fact]
        public void CircularEqualPartialRanking()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design
            {
                AccessibilityScore = 10.0f,
                HighestTemperatureScore = 10.0f,
                DesiredTemperatureScore = 10.0f,
                SpecificityScore = 10.0f,
                StructureScore = 10.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design two = new Design
            {
                AccessibilityScore = 15.0f,
                HighestTemperatureScore = 15.0f,
                DesiredTemperatureScore = 15.0f,
                SpecificityScore = 15.0f,
                StructureScore = 5.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design three = new Design
            {
                AccessibilityScore = 5.0f,
                HighestTemperatureScore = 12.05f,
                DesiredTemperatureScore = 20.0f,
                SpecificityScore = 5.0f,
                StructureScore = 10.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            List<Design> designs = new List<Design>
            {
                one,
                two,
                three
            };

            multiObjectiveOptimizer.Optimize(designs, 1);

            Assert.Equal(1, one.Rank);
            Assert.Equal(1, two.Rank);
            Assert.Equal(1, three.Rank);
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
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design two = new Design
            {
                AccessibilityScore = 2.0f,
                HighestTemperatureScore = 0.0f,
                DesiredTemperatureScore = 2.0f,
                SpecificityScore = 2.0f,
                StructureScore = 2.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
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
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design two = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                SpecificityScore = 1.0f,
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design three = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
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

        [Fact]
        public void InvalidFitnessCriteriaCount()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizer multiObjectiveOptimizer = new MultiObjectiveOptimization.MultiObjectiveOptimizer();

            Design one = new Design
            {
                AccessibilityScore = 1.0f,
                HighestTemperatureScore = 1.0f,
                DesiredTemperatureScore = 1.0f,
                StructureScore = 1.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            Design two = new Design
            {
                AccessibilityScore = 2.0f,
                HighestTemperatureScore = 0.0f,
                DesiredTemperatureScore = 2.0f,
                SpecificityScore = 2.0f,
                StructureScore = 2.0f,
                Job = new Job
                {
                    DesiredTempTolerance = 0.05f,
                    HighestTempTolerance = 0.05f,
                    SpecificityTolerance = 0.05f,
                    AccessibilityTolerance = 0.05f,
                    StructureTolerance = 0.05f
                }
            };

            List<Design> designs = new List<Design>
            {
                one,
                two
            };

            try {
                multiObjectiveOptimizer.Optimize(designs, 1);
            } catch(MultiObjectiveOptimization.MultiObjectiveOptimizationException Exception) {
                Assert.Equal(R_STATUS.R_FITNESS_VALUE_LENGTHS_DIFFER, Exception.Code);
                Assert.Equal("Candidates have different number of fitness values!", Exception.Message);
            }
        }

        [Fact]
        public void TestMultiObjectiveOptimizationExceptionEdgeCases()
        {
            MultiObjectiveOptimization.MultiObjectiveOptimizationException ex = new MultiObjectiveOptimization.MultiObjectiveOptimizationException();
            MultiObjectiveOptimization.MultiObjectiveOptimizationException cp = new MultiObjectiveOptimization.MultiObjectiveOptimizationException(0, "copy", ex);

            Assert.Equal(0, (double)cp.Code);
            Assert.Equal("copy", cp.Message);
        }
    }
}
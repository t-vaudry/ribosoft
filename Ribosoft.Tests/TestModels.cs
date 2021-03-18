using Xunit;
using Ribosoft.Models;
using Ribosoft.Models.JobsViewModels;

namespace Ribosoft.Tests
{
    public class TestModels
    {
        [Fact]
        public void TestAssemblyModel()
        {
            Assembly assembly = new Assembly();
            Assert.NotNull(assembly);
        }

        [Fact]
        public void TestDesignModel()
        {
            Design design = new Design();
            design.Job = new Job();
            design.Job.RNAInput = "AUGUGACUGUGAUCGUAAGUCGUAGGUCGUAAUGAGUGAUGCGUG";
            design.CutsiteIndex = 5;
            design.SubstrateSequenceLength = 10;

            Assert.Equal("ACUGUGAUCG", design.SubstrateTargetSequence);
        }

        [Fact]
        public void TestErrorViewModel()
        {
            ErrorViewModel vm = new ErrorViewModel();
            Assert.NotNull(vm);
            Assert.False(vm.ShowRequestId);
        }

        [Fact]
        public void TestJobModel()
        {
            Job job = new Job();
            job.JobState = JobState.Started;

            Assert.True(job.IsInProgress());
            Assert.False(job.IsCompleted());
        }

        [Fact]
        public void TestRibozymeModel()
        {
            Ribozyme ribozyme = new Ribozyme();
            Assert.NotNull(ribozyme);
        }

        [Fact]
        public void TestJobDetailsViewModel()
        {
            JobDetailsViewModel vm = new JobDetailsViewModel();
            JobDetailsViewModel.Filter filter = new JobDetailsViewModel.Filter();
            filter.param = "Rank";
            filter.condition = "eq";
            filter.value = "1";

            Assert.Equal("{\r\n  \"field\": {\r\n    \"label\": \"Rank\",\r\n    \"value\": \"Rank\"\r\n  },\r\n  \"operator\": {\r\n    \"label\": \"=\",\r\n    \"value\": \"eq\"\r\n  },\r\n  \"value\": {\r\n    \"label\": \"1\",\r\n    \"value\": \"1\"\r\n  }\r\n}", filter.GetJson());

            filter.param = "HighestTemperatureScore";
            filter.condition = "ne";
            Assert.Equal("{\r\n  \"field\": {\r\n    \"label\": \"Highest Temperature Score\",\r\n    \"value\": \"HighestTemperatureScore\"\r\n  },\r\n  \"operator\": {\r\n    \"label\": \"!=\",\r\n    \"value\": \"ne\"\r\n  },\r\n  \"value\": {\r\n    \"label\": \"1\",\r\n    \"value\": \"1\"\r\n  }\r\n}", filter.GetJson());

            filter.param = "DesiredTemperatureScore";
            filter.condition = "gt";
            Assert.Equal("{\r\n  \"field\": {\r\n    \"label\": \"Desired Temperature Score\",\r\n    \"value\": \"DesiredTemperatureScore\"\r\n  },\r\n  \"operator\": {\r\n    \"label\": \">\",\r\n    \"value\": \"gt\"\r\n  },\r\n  \"value\": {\r\n    \"label\": \"1\",\r\n    \"value\": \"1\"\r\n  }\r\n}", filter.GetJson());

            filter.param = "AccessibilityScore";
            filter.condition = "lt";
            Assert.Equal("{\r\n  \"field\": {\r\n    \"label\": \"Accessibility Score\",\r\n    \"value\": \"AccessibilityScore\"\r\n  },\r\n  \"operator\": {\r\n    \"label\": \"<\",\r\n    \"value\": \"lt\"\r\n  },\r\n  \"value\": {\r\n    \"label\": \"1\",\r\n    \"value\": \"1\"\r\n  }\r\n}", filter.GetJson());

            filter.param = "SpecificityScore";
            filter.condition = "";
            Assert.Equal("{\r\n  \"field\": {\r\n    \"label\": \"Specificity Score\",\r\n    \"value\": \"SpecificityScore\"\r\n  },\r\n  \"operator\": {\r\n    \"label\": \"\",\r\n    \"value\": \"\"\r\n  },\r\n  \"value\": {\r\n    \"label\": \"1\",\r\n    \"value\": \"1\"\r\n  }\r\n}", filter.GetJson());

            filter.param = "StructureScore";
            Assert.Equal("{\r\n  \"field\": {\r\n    \"label\": \"Structure Score\",\r\n    \"value\": \"StructureScore\"\r\n  },\r\n  \"operator\": {\r\n    \"label\": \"\",\r\n    \"value\": \"\"\r\n  },\r\n  \"value\": {\r\n    \"label\": \"1\",\r\n    \"value\": \"1\"\r\n  }\r\n}", filter.GetJson());
        }

        [Fact]
        public void TestJobIndexViewModel()
        {
            JobIndexViewModel vm = new JobIndexViewModel();
            Assert.NotNull(vm);
        }
    }
}

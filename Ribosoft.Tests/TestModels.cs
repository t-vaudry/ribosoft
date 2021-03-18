using Xunit;
using Ribosoft.Models;
using Ribosoft.Models.JobsViewModels;
using System;

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

            string le = "";
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;
            switch (pid)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    le = "\n";
                    break;
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                default:
                    le = "\r\n";
                    break;
            }

            Assert.Equal("{{0}  \"field\": {{0}    \"label\": \"Rank\",{0}    \"value\": \"Rank\"{0}  },{0}  \"operator\": {{0}    \"label\": \"=\",{0}    \"value\": \"eq\"{0}  },{0}  \"value\": {{0}    \"label\": \"1\",{0}    \"value\": \"1\"{0}  }{0}}".Replace("{0}", le), filter.GetJson());

            filter.param = "HighestTemperatureScore";
            filter.condition = "ne";
            Assert.Equal("{{0}  \"field\": {{0}    \"label\": \"Highest Temperature Score\",{0}    \"value\": \"HighestTemperatureScore\"{0}  },{0}  \"operator\": {{0}    \"label\": \"!=\",{0}    \"value\": \"ne\"{0}  },{0}  \"value\": {{0}    \"label\": \"1\",{0}    \"value\": \"1\"{0}  }{0}}".Replace("{0}", le), filter.GetJson());

            filter.param = "DesiredTemperatureScore";
            filter.condition = "gt";
            Assert.Equal("{{0}  \"field\": {{0}    \"label\": \"Desired Temperature Score\",{0}    \"value\": \"DesiredTemperatureScore\"{0}  },{0}  \"operator\": {{0}    \"label\": \">\",{0}    \"value\": \"gt\"{0}  },{0}  \"value\": {{0}    \"label\": \"1\",{0}    \"value\": \"1\"{0}  }{0}}".Replace("{0}", le), filter.GetJson());

            filter.param = "AccessibilityScore";
            filter.condition = "lt";
            Assert.Equal("{{0}  \"field\": {{0}    \"label\": \"Accessibility Score\",{0}    \"value\": \"AccessibilityScore\"{0}  },{0}  \"operator\": {{0}    \"label\": \"<\",{0}    \"value\": \"lt\"{0}  },{0}  \"value\": {{0}    \"label\": \"1\",{0}    \"value\": \"1\"{0}  }{0}}".Replace("{0}", le), filter.GetJson());

            filter.param = "SpecificityScore";
            filter.condition = "";
            Assert.Equal("{{0}  \"field\": {{0}    \"label\": \"Specificity Score\",{0}    \"value\": \"SpecificityScore\"{0}  },{0}  \"operator\": {{0}    \"label\": \"\",{0}    \"value\": \"\"{0}  },{0}  \"value\": {{0}    \"label\": \"1\",{0}    \"value\": \"1\"{0}  }{0}}".Replace("{0}", le), filter.GetJson());

            filter.param = "StructureScore";
            Assert.Equal("{{0}  \"field\": {{0}    \"label\": \"Structure Score\",{0}    \"value\": \"StructureScore\"{0}  },{0}  \"operator\": {{0}    \"label\": \"\",{0}    \"value\": \"\"{0}  },{0}  \"value\": {{0}    \"label\": \"1\",{0}    \"value\": \"1\"{0}  }{0}}".Replace("{0}", le), filter.GetJson());
        }

        [Fact]
        public void TestJobIndexViewModel()
        {
            JobIndexViewModel vm = new JobIndexViewModel();
            Assert.NotNull(vm);
        }
    }
}

using Ribosoft.GenbankRequests;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestValidGenbankRequests
    {
        [Fact]
        public async void TestValidAccessionId()
        {
            GenbankResult result = await GenbankRequest.RunSequenceRequest("M55055");
            Assert.Equal("UCAGUUUAAACCUCACACAGAUGAAGACCUUGUCUACUUGGAUUCCAGCCCAGAUUUCUGUGAUCAUGACCUAAAGAAUGGGGUCUUAGGUACAACUGGUCGGCACUGUAACAAGACUUCCAAAGCUAUAGAUGGCUGUGAGCUCAUGUGCUGUGGAAGAGGAUUUCACACGGAAGAGGUUGAGAUAGUAGAGAGGUGUAGUUGCAAAUUUCACUGGUGUU", result.Sequence);
            Assert.Equal(0, result.OpenReadingFrameStart);
            Assert.Equal(220, result.OpenReadingFrameEnd);
        }
    }
}

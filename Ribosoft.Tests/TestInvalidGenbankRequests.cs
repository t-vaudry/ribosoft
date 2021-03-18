using Ribosoft.GenbankRequests;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestInvalidGenbankRequests
    {
        [Fact]
        public async System.Threading.Tasks.Task TestInvalidAccessionId()
        {
            GenbankRequestsException ex = await Assert.ThrowsAsync<GenbankRequestsException>(() => GenbankRequest.RunSequenceRequest("45"));
            Assert.Equal("The accession ID does not exist.", ex.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task TestInvalidReturnSequence()
        {
            GenbankRequestsException ex = await Assert.ThrowsAsync<GenbankRequestsException>(() => GenbankRequest.RunSequenceRequest("M74443"));
            Assert.Equal("A non-base character N was found in the sequence retrieved.", ex.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task TestErrorException()
        {
            GenbankRequestsException ex = await Assert.ThrowsAsync<GenbankRequestsException>(() => GenbankRequest.RunSequenceRequest("M63332"));
            Assert.Equal("An error occurred with the GenBank request.", ex.Message);
        }
    }
}

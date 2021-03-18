using Xunit;

namespace Ribosoft.Tests
{
    public class TestError
    {
        [Fact]
        public void TestRibosoftExceptionCtor()
        {
            RibosoftException ex = new RibosoftException();
            Assert.Equal(-999, (double)ex.Code);
            Assert.Empty(ex.Message);
        }
    }
}

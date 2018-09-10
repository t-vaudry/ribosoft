using System;
using Xunit;
using Ribosoft;

namespace Ribosoft.Tests
{
    public class UnitTest
    {
        [Fact]
        public void Test()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Assert.Equal(R_STATUS.R_STATUS_OK, sdc.ValidateSequence("AUUGCC"));
        }
    }
}

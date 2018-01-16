using System;
using Xunit;
using Ribosoft;

namespace Ribosoft.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Assert.Equal(3, sdc.Add(1, 2));
        }

        [Fact]
        public void Test2()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Assert.Equal(R_STATUS.R_STATUS_OK, sdc.ValidateSequence("AUUGCC"));
        }
    }
}

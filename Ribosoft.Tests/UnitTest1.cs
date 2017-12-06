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
            SampleDllCall sdc = new SampleDllCall();
            Assert.Equal(3, sdc.Add(1, 2));
        }

        [Fact]
        public void Test2()
        {
            SampleDllCall sdc = new SampleDllCall();
            Assert.Equal(R_STATUS.R_OK, sdc.ValidateSequence("AUUGCC"));
        }
    }
}

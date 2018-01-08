using System;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestFolding
    {
        [Fact]
        public void TestFolding_Valid()
        {
            //FoldOutput[] output = new FoldOutput[1];
            SampleDllCall sdc = new SampleDllCall();

            IntPtr output;
            int size;

            sdc.Fold("AUGUCUUAGGUGAUACGUGC", out output, out size);

            Assert.Equal(".((((......)))).....", output[0].Structure);
        }
    }
}
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestFolding
    {
        [Fact]
        public void TestFolding_Valid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            var data = sdc.Fold("AUGUCUUAGGUGAUACGUGC");

            Assert.False(data == null);

            Assert.Equal(".((((......)))).....", data[0].Structure);
            Assert.Equal(-1.60f, data[0].Energy);

            Assert.Equal("((((..(.....).))))..", data[35].Structure);
            Assert.Equal(3.00f, data[35].Energy);
            
            Assert.Equal(51, data.Count);
        }

        [Fact]
        public void TestFolding_AnotherValid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            var data = sdc.Fold("AUUUUAGUGCUGAUGGCCAAUGCGCGAACCCAUCGGCGCUGUGA");

            Assert.False(data == null);

            Assert.Equal(".((.((((((((((((.............)))))))))))).))", data[1].Structure);
            Assert.Equal(-16.20f, data[1].Energy);

            Assert.Equal(".....((((((((((((........)...)))))))))))....", data[17].Structure);
            Assert.Equal(-14.50f, data[17].Energy);

            Assert.Equal(173, data.Count);
        }
    }
}
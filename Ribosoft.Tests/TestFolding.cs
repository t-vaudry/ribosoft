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
            Assert.Equal(0.66907f, data[0].Probability, 5);

            Assert.Equal("((((..(.....).))))..", data[35].Structure);
            Assert.Equal(0.00038f, data[35].Probability, 5);
            
            Assert.Equal(51, data.Count);
        }

        [Fact]
        public void TestFolding_AnotherValid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            var data = sdc.Fold("AUUUUAGUGCUGAUGGCCAAUGCGCGAACCCAUCGGCGCUGUGA");

            Assert.False(data == null);

            Assert.Equal(".((.((((((((((((.............)))))))))))).))", data[1].Structure);
            Assert.Equal(0.11585f, data[1].Probability, 5);

            Assert.Equal(".....((((((((((((........)...)))))))))))....", data[17].Structure);
            Assert.Equal(0.00734f, data[17].Probability, 5);

            Assert.Equal(173, data.Count);
        }
    }
}
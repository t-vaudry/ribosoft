using Xunit;
using Ribosoft.Biology;
using System;

namespace Ribosoft.Tests
{
    public class TestBiology
    {
        [Fact]
        public void TestNucleotideEdgeCases()
        {
            Nucleotide defNucleo = new Nucleotide();
            Nucleotide cpNucleo = new Nucleotide(defNucleo);
            Nucleotide nucleotide = new Nucleotide('T');

            Exception ex = Assert.Throws<RibosoftException>(() => nucleotide.GetComplement());
            Assert.Equal("Cannot get complement of invalid symbol T", ex.Message);

            ex = Assert.Throws<RibosoftException>(() => nucleotide.GetSpecialComplements());
            Assert.Equal("Cannot get complement of invalid symbol T", ex.Message);
        }

        [Fact]
        public void TestSequenceEdgeCases()
        {
            Sequence sequence = new Sequence("AUGCAAGCAUUGCU");
            Assert.Equal('G', sequence.GetCharAt(2));
        }

        [Fact]
        public void TestSubstrateInfoEdgeCases()
        {
            SubstrateInfo substrateInfo = new SubstrateInfo();
            Assert.Equal(0, substrateInfo.CutsiteOffset);
        }
    }
}

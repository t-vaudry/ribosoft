using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;
using Ribosoft.Models;

namespace Ribosoft.Tests
{
    public class TestRibosoftAlgo
    {
        [Fact]
        public void TestDefaultFolding_Valid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            var data = sdc.MFEFold("AUGUCUUAGGUGAUACGUGC");

            Assert.False(data == null);
            Assert.Equal(".((((......)))).....", data);
        }

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

        [Fact]
        public void TestDefaultFolding_Invalid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            Exception ex = Assert.Throws<RibosoftAlgoException>(() => sdc.MFEFold("AUGUXWQD"));
        }

        [Fact]
        public void TestFolding_Invalid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            Exception ex = Assert.Throws<RibosoftAlgoException>(() => sdc.Fold("AUGUXWQD"));
        }

        [Fact]
        public void TestValidateSequence()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            var code = sdc.ValidateSequence("AUGACGUGAUGCUAGA");
            Assert.Equal(0, (double)code);
        }

        [Fact]
        public void TestValidateStructure()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();

            var code = sdc.ValidateStructure("...()..(())(())...");
            Assert.Equal(0, (double)code);
        }

        [Fact]
        public void TestAccessibility()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();
            candidate.SubstrateSequence = "UUGUUGU";
            candidate.SubstrateStructure = "43..210";

            float val = sdc.Accessibility(candidate, "......((((..(((...)))..))))......", 11, 1.0f, 0.05f, 22.0f);
            Assert.Equal(1430258.25f, val);
        }

        [Fact]
        public void TestAnnealing()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();

            float val = sdc.Anneal(candidate, "AAUUUCCCCGGGGG", "0123abxyzABXYZ", 1.0f, 0.05f, 22.0f);
            Assert.Equal(4570.36865f, val);
        }

        [Fact]
        public void TestStructure()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Design design = new Design();
            design.Sequence = "AUGCACGU";
            design.IdealStructure = ".(.().).";
            IList<Design> designList = new List<Design>();
            designList.Add(design);

            sdc.Structure(designList);
            Assert.Equal(1.0f, designList[0].StructureScore);
        }

        [Fact]
        public void TestAccessibilityInvalid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();
            candidate.SubstrateSequence = "UUGUXGU";
            candidate.SubstrateStructure = "43..210";

            Assert.Throws<RibosoftAlgoException>(() => sdc.Accessibility(candidate, "......((((..(((...)))..))))......", 11, 1.0f, 0.5f, 22.0f));
        }

        [Fact]
        public void TestAnnealingInvalid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();

            Assert.Throws<RibosoftAlgoException>(() => sdc.Anneal(candidate, "AAUUUCCHJSGGGG", "0123abxyzABXYZ", 1.0f, 0.05f, 22.0f));
        }

        [Fact]
        public void TestStructureInvalid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Design design = new Design();
            design.Sequence = "AUGCACGU";
            design.IdealStructure = ".)(.)...";
            IList<Design> designList = new List<Design>();
            designList.Add(design);

            Assert.Throws<RibosoftAlgoException>(() => sdc.Structure(designList));
        }

        [Fact]
        public void TestRibosoftAlgoExceptionEdgeCases()
        {
            RibosoftAlgoException exception = new RibosoftAlgoException();
            RibosoftAlgoException exception1 = new RibosoftAlgoException(0);

            Assert.Equal(-999, (double)exception.Code);
            Assert.Equal(0, (double)exception1.Code);
        }
    }
}
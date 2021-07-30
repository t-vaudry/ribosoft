using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestRibosoftAlgo
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
            candidate.CutsiteIndices = new List<int>();
            candidate.CutsiteIndices.Add(11);

            float val = sdc.Accessibility(candidate, "CUUGAAGUGGUUUGUUGUGCUUGAAGAGACCCC", 4);
            Assert.Equal(3.3999999f, val);
        }

        [Fact]
        public void TestAnnealing()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();

            float val = sdc.Anneal(candidate, "AAUUUCCCCGGGGG", "0123abxyzABXYZ", 1.0f, 0.05f, 37.0f);
            Assert.Equal(89.6045f, val);
        }

        [Fact]
        public void TestStructure()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();
            candidate.Sequence = new Biology.Sequence("AUGCACGU");

            float val = sdc.Structure(candidate, ".(.().).");
            Assert.Equal(5.9977207f, val);
        }

        [Fact]
        public void TestAccessibilityInvalid()
        {
            RibosoftAlgo sdc = new RibosoftAlgo();
            Candidate candidate = new Candidate();
            candidate.SubstrateSequence = "UUGUXGU";
            candidate.CutsiteIndices = new List<int>();
            candidate.CutsiteIndices.Add(77);

            Assert.Throws<RibosoftAlgoException>(() => sdc.Accessibility(candidate, "CUUGAAGUGGUUUGUUGUGCUUGAAGAGACCCC", 4));
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
            Candidate candidate = new Candidate();
            candidate.Sequence = new Biology.Sequence("AUGCACGU");

            Assert.Throws<RibosoftAlgoException>(() => sdc.Structure(candidate, ".)(.)..."));
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
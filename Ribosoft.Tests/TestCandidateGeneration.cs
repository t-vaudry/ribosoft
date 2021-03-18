using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestCandidateGeneration
    {
        [Fact]
        public void Generate_Candidates_Valid_Input()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList();
            candidateGenerator.Clear();

            Assert.Single(candidates);
            Assert.Equal("CGUGGUUAGGGCCACGUUAAAUAGUUAUUUAAGCCCUAAGCGUGCAAU", candidates[0].Sequence.GetString());
            Assert.Equal("AUUGCAGUAUAA", candidates[0].SubstrateSequence);
            Assert.Equal("987654..3210", candidates[0].SubstrateStructure);
        }

        [Fact]
        public void Generate_Candidates_Unequal_Ribozyme_Length()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Ribozyme sequence length does not match ribozyme structure length.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Unequal_Substrate_Length()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..321",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Substrate sequence length does not match substrate structure length.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Invalid_Ribozyme_Structure()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((&[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Unrecognized structure symbol encountered.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Invalid_Substrate_Structure()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654&.3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Unexpected substrate structure character: &", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Unmatched_Substrate_Structure()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "a87654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Substrate structure character not found in ribozyme structure: a", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Unmatched_Neighbour()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACCUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Neighbours don't match!", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Invalid_Nucleotide()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<RibosoftException>(() => candidateGenerator.GenerateCandidates(
                "QGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Invalid nucleotide base Q was provided.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Unclosed_Bond()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))).........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Unclosed bond found '('. Input may be faulty.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Unopened_Pseudoknot()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<InvalidOperationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[..))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
        }

        [Fact]
        public void Generate_Candidates_Unclosed_Pseudoknot()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]....456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Unclosed pseudoknot found '{'. Input may be faulty.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Multiple_Cutsites()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "N",
                "0",
                "NGU",
                "0..",
                "AGUAGUCGUGGUUGU").ToList();
            candidateGenerator.Clear();

            Assert.Equal(4, candidates.Count);
            Assert.Equal("U", candidates[0].Sequence.GetString());
            Assert.Equal("G", candidates[1].Sequence.GetString());
            Assert.Equal("C", candidates[2].Sequence.GetString());
            Assert.Equal("A", candidates[3].Sequence.GetString());

            Assert.Equal(2, candidates[0].CutsiteIndices.Count);
            Assert.Equal(0, candidates[0].CutsiteIndices[0]);
            Assert.Equal(3, candidates[0].CutsiteIndices[1]);
        }

        [Fact]
        public void Generate_Candidates_Variable_First_Neighbour()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "NR",
                "()",
                "UAUACGGC",
                "........",
                "UAUACGGCAUUGCAGUAUAAAGCCU").ToList();
            candidateGenerator.Clear();

            Assert.Equal(3, candidates.Count);
            Assert.Equal("CG", candidates[0].Sequence.GetString());
            Assert.Equal("UA", candidates[1].Sequence.GetString());
            Assert.Equal("UG", candidates[2].Sequence.GetString());
        }

        [Fact]
        public void Generate_Candidates_Middle_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGnnNNNN",
                "((((.[[[[[..))))........0123.....]]]]]]...456789",
                "NNNNnnGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Repeat notation not located at beginning or end of substrate sequence. Case not supported.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_No_Target_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[..))))........0123.....]]]]]]...456789",
                "NNNNNNGnNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Repeat notation is not on target. Unhandled case.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Internal_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNnNNNN",
                "((((.[[[[[..))))........0123.....]]]]]]...456789",
                "NNNNnNGNNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("Repeat notation not located at beginning or end of substrate sequence. Case not supported.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Multiple_Repeat_Regions()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGnnNNUUAAGCCCUAAGCGNNNNnn",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "nnNNNNnNNNnn",
                "987654a.3210",
                "AUUGCAGUAUAAAGCCU").ToList());
            candidateGenerator.Clear();

            Assert.Equal("More than 2 repeat regions are not supported.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_End_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "GnNA",
                ".01.",
                "Nn",
                "10",
                "AU").ToList();
            candidateGenerator.Clear();

            Assert.Equal(3, candidates.Count);
            Assert.Equal("GUA", candidates[0].Sequence.GetString());
            Assert.Equal("A", candidates[0].SubstrateSequence);
            Assert.Equal("1", candidates[0].SubstrateStructure);
            Assert.Equal("GAA", candidates[1].Sequence.GetString());
            Assert.Equal("U", candidates[1].SubstrateSequence);
            Assert.Equal("1", candidates[1].SubstrateStructure);
            Assert.Equal("GAUA", candidates[2].Sequence.GetString());
            Assert.Equal("AU", candidates[2].SubstrateSequence);
            Assert.Equal("10", candidates[2].SubstrateStructure);
        }

        [Fact]
        public void Generate_Candidates_Start_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "GNnA",
                ".01.",
                "nN",
                "10",
                "AU").ToList();
            candidateGenerator.Clear();

            Assert.Equal(3, candidates.Count);
            Assert.Equal("GUA", candidates[0].Sequence.GetString());
            Assert.Equal("A", candidates[0].SubstrateSequence);
            Assert.Equal("0", candidates[0].SubstrateStructure);
            Assert.Equal("GAA", candidates[1].Sequence.GetString());
            Assert.Equal("U", candidates[1].SubstrateSequence);
            Assert.Equal("0", candidates[1].SubstrateStructure);
            Assert.Equal("GAUA", candidates[2].Sequence.GetString());
            Assert.Equal("AU", candidates[2].SubstrateSequence);
            Assert.Equal("10", candidates[2].SubstrateStructure);
        }

        [Fact]
        public void Generate_Candidates_StartEnd_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "nGNn",
                "0.12",
                "nNn",
                "210",
                "AUG").ToList();
            candidateGenerator.Clear();

            Assert.Equal(8, candidates.Count);
            Assert.Equal("GU", candidates[0].Sequence.GetString());
            Assert.Equal("A", candidates[0].SubstrateSequence);
            Assert.Equal("1", candidates[0].SubstrateStructure);
            Assert.Equal("GC", candidates[1].Sequence.GetString());
            Assert.Equal("G", candidates[1].SubstrateSequence);
            Assert.Equal("1", candidates[1].SubstrateStructure);
            Assert.Equal("GA", candidates[2].Sequence.GetString());
            Assert.Equal("U", candidates[2].SubstrateSequence);
            Assert.Equal("1", candidates[2].SubstrateStructure);
            Assert.Equal("GAU", candidates[3].Sequence.GetString());
            Assert.Equal("AU", candidates[3].SubstrateSequence);
            Assert.Equal("21", candidates[3].SubstrateStructure);
            Assert.Equal("GCA", candidates[4].Sequence.GetString());
            Assert.Equal("UG", candidates[4].SubstrateSequence);
            Assert.Equal("21", candidates[4].SubstrateStructure);
            Assert.Equal("CGA", candidates[5].Sequence.GetString());
            Assert.Equal("UG", candidates[5].SubstrateSequence);
            Assert.Equal("10", candidates[5].SubstrateStructure);
            Assert.Equal("CGAU", candidates[6].Sequence.GetString());
            Assert.Equal("AUG", candidates[6].SubstrateSequence);
            Assert.Equal("210", candidates[6].SubstrateStructure);
            Assert.Equal("AGU", candidates[7].Sequence.GetString());
            Assert.Equal("AU", candidates[7].SubstrateSequence);
            Assert.Equal("10", candidates[7].SubstrateStructure);
        }

        [Fact]
        public void Generate_Candidates_Yarrowia()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnnnnnnnnnnn",
                "01234567.......((........89abcdef..))...ghijklmnopqrstu",
                "nnnnnnnnnnnNNGUCGNNNNNNNNNNNNNNN",
                "utsrqponmlkjihg.76543210fedcba98",
                "AUGUCGGUCGAUAUGCUAGCUAGCUAGAGUC").ToList();
            candidateGenerator.Clear();

            Assert.Equal(6, candidates.Count);
            Assert.Equal("AUGUCGGUCGAUAUGCUAGCU", candidates[0].SubstrateSequence);
            Assert.Equal("CGGUCGAUAUGCUAGCUAGCU", candidates[1].SubstrateSequence);
            Assert.Equal("UCGGUCGAUAUGCUAGCUAGCU", candidates[2].SubstrateSequence);
            Assert.Equal("GUCGGUCGAUAUGCUAGCUAGCU", candidates[3].SubstrateSequence);
            Assert.Equal("UGUCGGUCGAUAUGCUAGCUAGCU", candidates[4].SubstrateSequence);
            Assert.Equal("AUGUCGGUCGAUAUGCUAGCUAGCU", candidates[5].SubstrateSequence);
        }

        [Fact]
        public void Generate_Candidates_Repeat_Test()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnn",
                "01234567.......((........89abcdef..))...ghijkl",
                "nnNNGUCGNNNNNNNNNNNNNNN",
                "lkjihg.76543210fedcba98",
                "AUGUCGGUCGAUAUGCUAGCUAGCUAGAGUC").ToList();
            candidateGenerator.Clear();

            Assert.Equal(4, candidates.Count);
            Assert.Equal("UAUCGACCCUGAUGAGAACAAACCCAGCUAGCACGUCGAAACAU", candidates[0].Sequence.GetString());
            Assert.Equal("AGCAUAUCCUGAUGAGAACAAACCCAGCUAGCUCGUCGAAACCGAC", candidates[3].Sequence.GetString());
        }

        [Fact]
        public void Generate_Candidates_Bond_Substrate()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "NNNNNNNCCUGAUGAGAACAAACCCNNNNNNNNCGUCGAAACNNnn",
                "01234567.......((........89abcdef..))...gh..kl",
                "nnNNGUCGNNNNNNNNNNNNNNN",
                "lk..hg.76543210fedcba98",
                "AUGUCGGUCGAUAUGCUAGCUAGCUAGAGUC").ToList();
            candidateGenerator.Clear();

            Assert.Equal(64, candidates.Count);
            Assert.Equal("UAUCGACCCUGAUGAGAACAAACCCAGCUAGCACGUCGAAACAA", candidates[0].Sequence.GetString());
            Assert.Equal("UAUCGACCCUGAUGAGAACAAACCCAGCUAGCACGUCGAAACAC", candidates[1].Sequence.GetString());
        }

        [Fact]
        public void Generate_Candidates_GU_Pairing()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "GN",
                "()",
                "N",
                ".",
                "A"
                ).ToList();
            candidateGenerator.Clear();

            Assert.Equal(2, candidates.Count);
            Assert.Equal("GC", candidates[0].Sequence.GetString());
            Assert.Equal("GU", candidates[1].Sequence.GetString());
        }

        [Fact]
        public void Generate_Candidates_All_Indices_Of_Nothing()
        {
            Exception ex = Assert.Throws<ArgumentException>(() => CandidateGeneration.CandidateGenerator.AllIndicesOf("", ""));

            Assert.Equal("the string to find may not be empty (Parameter 'value')", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Exceptions_Edge_Cases()
        {
            CandidateGeneration.CandidateGenerationException ex = new CandidateGeneration.CandidateGenerationException();
            CandidateGeneration.CandidateGenerationException cp = new CandidateGeneration.CandidateGenerationException("copy", ex);

            Assert.Equal("copy", cp.Message);
        }

        [Fact]
        public void Generate_Candidates_Node_Edge_Cases()
        {
            CandidateGeneration.Node node = new CandidateGeneration.Node();
            Assert.Equal(-1, node.Depth);
        }

        [Fact]
        public void Generate_Candidates_Ribozyme_Edge_Cases()
        {
            CandidateGeneration.Ribozyme ribozyme = new CandidateGeneration.Ribozyme();
            Assert.Null(ribozyme.Sequence);
            Assert.Null(ribozyme.Structure);
            Assert.Null(ribozyme.SubstrateSequence);
            Assert.Null(ribozyme.SubstrateStructure);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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

            Assert.Single(candidates);
            Assert.Equal("CGUGGUUAGGGCCACGUUAAAUAGUUAUUUAAGCCCUAAGCGUGCAAU", candidates[0].Sequence.GetString());
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
        public void Generate_Candidates_Multiple_Cutsites()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            var candidates = candidateGenerator.GenerateCandidates(
                "N",
                "0",
                "NGU",
                "0..",
                "AGUAGUCGUGGUUGU").ToList();

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

            Assert.Equal(2, candidates.Count);
            Assert.Equal("CG", candidates[0].Sequence.GetString());
            Assert.Equal("UA", candidates[1].Sequence.GetString());
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
            Assert.Equal("Repeat notation is not on target. Unhandled case.", ex.Message);
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

            Assert.Equal(3, candidates.Count);
            Assert.Equal("GUA", candidates[0].Sequence.GetString());
            Assert.Equal("GAA", candidates[1].Sequence.GetString());
            Assert.Equal("GUAA", candidates[2].Sequence.GetString());
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

            Assert.Equal(3, candidates.Count);
            Assert.Equal("GUA", candidates[0].Sequence.GetString());
            Assert.Equal("GAA", candidates[1].Sequence.GetString());
            Assert.Equal("GAUA", candidates[2].Sequence.GetString());
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

            Assert.Equal(8, candidates.Count);
            Assert.Equal("GU", candidates[0].Sequence.GetString());
            Assert.Equal("GC", candidates[1].Sequence.GetString());
            Assert.Equal("GA", candidates[2].Sequence.GetString());
            Assert.Equal("GAU", candidates[3].Sequence.GetString());
            Assert.Equal("GCA", candidates[4].Sequence.GetString());
            Assert.Equal("AGC", candidates[5].Sequence.GetString());
            Assert.Equal("UGCA", candidates[6].Sequence.GetString());
            Assert.Equal("UGA", candidates[7].Sequence.GetString());
        }
    }
}

using System;
using System.Collections.Generic;
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
                "AUUGCAGUAUAAAGCCU");

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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));

            Assert.Equal("Substrate structure character not found in ribozyme structure: a", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Unmatched_Neighbour()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<AggregateException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACCUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU"));

            Assert.Equal(typeof(CandidateGeneration.CandidateGenerationException), ex.InnerException.GetType());
            Assert.Equal("Neighbours don't match!", ex.InnerException.Message);
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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));
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
                "AGUAGUCGUGGUUGU");

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
                "UAUACGGCAUUGCAGUAUAAAGCCU");

            Assert.Equal(2, candidates.Count);
            Assert.Equal("CG", candidates[0].Sequence.GetString());
            Assert.Equal("UA", candidates[1].Sequence.GetString());
        }
    }
}

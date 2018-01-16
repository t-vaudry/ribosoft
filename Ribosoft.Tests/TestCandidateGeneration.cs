using System;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestCandidateGeneration
    {
        [Fact]
        public void Generate_Candidates_Valid_Input()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_STATUS_OK);
            Assert.Equal(1, result.RibozymCandidates.Count);
            Assert.Equal("CGUGGUUAGGGCCACGUUAAAUAGUUAUUUAAGCCCUAAGCGUGCAAU", result.RibozymCandidates[0].GetString());
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

            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((&[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_INVALID_STRUCT_ELEMENT);
        }

        [Fact]
        public void Generate_Candidates_Invalid_Substrate_Structure()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654&.3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_INVALID_STRUCT_ELEMENT);
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
            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "QGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_INVALID_NUCLEOTIDE);
        }

        [Fact]
        public void Generate_Candidates_Unclosed_Bond()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))).........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_BAD_PAIR_MATCH);
        }

        [Fact]
        public void Generate_Candidates_Unopened_Pseudoknot()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[..))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_BAD_PAIR_MATCH);
        }

        [Fact]
        public void Generate_Candidates_Multiple_Cutsites()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "N",
                "0",
                "NGU",
                "0..",
                "AGUAGUCGUGGUUGU");

            Assert.Equal(result.Status, R_STATUS.R_STATUS_OK);
            Assert.Equal(4, result.RibozymCandidates.Count);
            Assert.Equal("U", result.RibozymCandidates[0].GetString());
            Assert.Equal("G", result.RibozymCandidates[1].GetString());
            Assert.Equal("C", result.RibozymCandidates[2].GetString());
            Assert.Equal("A", result.RibozymCandidates[3].GetString());
        }

        [Fact]
        public void Generate_Candidates_Variable_First_Neighbour()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            CandidateGeneration.CandidateGenerationResult result = candidateGenerator.GenerateCandidates(
                "NR",
                "()",
                "UAUACGGC",
                "........",
                "UAUACGGCAUUGCAGUAUAAAGCCU");

            Assert.Equal(result.Status, R_STATUS.R_STATUS_OK);
            Assert.Equal(2, result.RibozymCandidates.Count);
            Assert.Equal("CG", result.RibozymCandidates[0].GetString());
            Assert.Equal("UA", result.RibozymCandidates[1].GetString());
        }
    }
}

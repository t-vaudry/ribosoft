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
            candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGNNNNUUAAGCCCUAAGCGNNNNNN",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "NNNNNNGUNNNN",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU");

            Assert.Equal(1, candidateGenerator.SequencesToSend.Count);
            Assert.Equal("CGUGGUUAGGGCCACGUUAAAUAGUUAUUUAAGCCCUAAGCGUGCAAU", candidateGenerator.SequencesToSend[0].GetString());
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

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
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
    }
}

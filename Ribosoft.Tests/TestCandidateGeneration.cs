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

        [Fact]
        public void Generate_Candidates_Multiple_Cutsites()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            candidateGenerator.GenerateCandidates(
                "N",
                "0",
                "NGU",
                "0..",
                "AGUAGUCGUGGUUGU");

            Assert.Equal(4, candidateGenerator.SequencesToSend.Count);
            Assert.Equal("U", candidateGenerator.SequencesToSend[0].GetString());
            Assert.Equal("G", candidateGenerator.SequencesToSend[1].GetString());
            Assert.Equal("C", candidateGenerator.SequencesToSend[2].GetString());
            Assert.Equal("A", candidateGenerator.SequencesToSend[3].GetString());
        }

        [Fact]
        public void Generate_Candidates_Variable_First_Neighbour()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            candidateGenerator.GenerateCandidates(
                "NR",
                "()",
                "UAUACGGC",
                "........",
                "UAUACGGCAUUGCAGUAUAAAGCCU");

            Assert.Equal(2, candidateGenerator.SequencesToSend.Count);
            Assert.Equal("CG", candidateGenerator.SequencesToSend[0].GetString());
            Assert.Equal("UA", candidateGenerator.SequencesToSend[1].GetString());
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
                "AUUGCAGUAUAAAGCCU"));

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
                "AUUGCAGUAUAAAGCCU"));
            Assert.Equal("Repeat notation is not on target. Unhandled case.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_Multiple_Repeat_Regions()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();

            Exception ex = Assert.Throws<CandidateGeneration.CandidateGenerationException>(() => candidateGenerator.GenerateCandidates(
                "CGUGGUUAGGGCCACGUUAAAUAGnnNNUUAAGCCCUAAGCGNNNNnn",
                "((((.[[[[[[.))))........0123.....]]]]]]...456789",
                "nnNNNNGNNNnn",
                "987654..3210",
                "AUUGCAGUAUAAAGCCU"));
            Assert.Equal("Multiple repeat regions are not supported.", ex.Message);
        }

        [Fact]
        public void Generate_Candidates_End_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            candidateGenerator.GenerateCandidates(
                "GnNA",
                ".01.",
                "Nn",
                "10",
                "AU");

            Assert.Equal(3, candidateGenerator.SequencesToSend.Count);
            Assert.Equal("GUA", candidateGenerator.SequencesToSend[0].GetString());
            Assert.Equal("GAA", candidateGenerator.SequencesToSend[1].GetString());
            Assert.Equal("GUAA", candidateGenerator.SequencesToSend[2].GetString());
        }

        [Fact]
        public void Generate_Candidates_Start_Repeat()
        {
            CandidateGeneration.CandidateGenerator candidateGenerator = new CandidateGeneration.CandidateGenerator();
            candidateGenerator.GenerateCandidates(
                "GNnA",
                ".01.",
                "nN",
                "10",
                "AU");

            Assert.Equal(3, candidateGenerator.SequencesToSend.Count);
            Assert.Equal("GUA", candidateGenerator.SequencesToSend[0].GetString());
            Assert.Equal("GAA", candidateGenerator.SequencesToSend[1].GetString());
            Assert.Equal("GAUA", candidateGenerator.SequencesToSend[2].GetString());
        }
    }
}

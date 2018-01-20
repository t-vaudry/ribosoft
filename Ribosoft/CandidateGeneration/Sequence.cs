using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Ribosoft.CandidateGeneration
{
    public class Sequence
    {
        public List<Nucleotide> Nucleotides { get; set; }
        public int Capacity;

        public Sequence()
        {
            Capacity = 0;
            Nucleotides = new List<Nucleotide>(Capacity);
        }

        public Sequence(int capacity)
        {
            Capacity = capacity;
            Nucleotides = new List<Nucleotide>(Capacity);
        }

        public Sequence(Sequence otherSequence)
        {
            Nucleotides = new List<Nucleotide>(otherSequence.Nucleotides);
        }

        public String GetString()
        {
            StringBuilder sb = new System.Text.StringBuilder();

            foreach (Nucleotide nucleotide in Nucleotides)
                sb.Append(nucleotide.Symbol);

            return sb.ToString();
        }

        public char GetCharAt(int index)
        {
            return Nucleotides[index].Symbol;
        }

        public String GetComplement()
        {
            StringBuilder complement = new System.Text.StringBuilder();

            foreach (Nucleotide n in Nucleotides)
                complement.Append(n.GetComplement());

            return complement.ToString();
        }

        public int GetLength()
        {
            return Nucleotides.Count;
        }

        public void Insert(int idx, Nucleotide nucleotide)
        {
            Nucleotides.Insert(idx, nucleotide);
        }
    }
}

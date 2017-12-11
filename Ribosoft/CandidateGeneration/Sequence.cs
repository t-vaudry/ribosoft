using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Ribosoft.CandidateGeneration
{
    class Sequence
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
            Debug.Assert(index > -1 && index < Nucleotides.Count);

            return Nucleotides[index].Symbol;
        }
    }
}

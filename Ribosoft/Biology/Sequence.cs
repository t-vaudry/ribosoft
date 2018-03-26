using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ribosoft.Biology
{
    public class Sequence
    {
        public List<Nucleotide> Nucleotides { get; set; }

        public Sequence()
        {
            Nucleotides = new List<Nucleotide>();
        }

        public Sequence(int capacity)
        {
            Nucleotides = new List<Nucleotide>(capacity);
        }

        public Sequence(Sequence otherSequence)
        {
            Nucleotides = new List<Nucleotide>(otherSequence.Nucleotides);
        }

        public Sequence(string sequence) : this()
        {
            Nucleotides = sequence.Select(c => new Nucleotide(c)).ToList();
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

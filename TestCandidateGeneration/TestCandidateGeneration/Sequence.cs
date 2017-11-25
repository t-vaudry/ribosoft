using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace TestCandidateGeneration
{
    class Sequence
    {
        public List<Nucleotide> mNucleotides;
        public int mCapacity;

        public Sequence()
        {
            mCapacity = 0;
            mNucleotides = new List<Nucleotide>(mCapacity);
        }

        public Sequence(int capacity)
        {
            mCapacity = capacity;
            mNucleotides = new List<Nucleotide>(mCapacity);
        }

        public Sequence(Sequence otherSequence)
        {
            mNucleotides = new List<Nucleotide>(otherSequence.mNucleotides);
        }

        public String GetString()
        {
            StringBuilder sb = new System.Text.StringBuilder();

            foreach (Nucleotide nucleotide in mNucleotides)
                sb.Append(nucleotide.mSymbol);

            return sb.ToString();
        }

        public char GetCharAt(int index)
        {
            Debug.Assert(index > -1 && index < mNucleotides.Count);

            return mNucleotides[index].mSymbol;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Ribosoft.Biology
{
    public class Nucleotide
    {
        public char Symbol { get; set; } = 'X';
        public List<char> Bases { get; set; }
        public bool IsBase { get; set; } = false;

        public Nucleotide()
        {
        }

        public Nucleotide(Nucleotide other)
        {
            Symbol = other.Symbol;
            Bases = other.Bases;
            IsBase = other.IsBase;
        }

        public Nucleotide(char symbol)
        {
            Bases = new List<char>();
            SetSymbol(symbol);
        }

        public void SetSymbol(char symbol)
        {
            Symbol = Char.ToUpper(symbol);
            IsBase = Symbol.Equals('A') || Symbol.Equals('C') || Symbol.Equals('G') || Symbol.Equals('U');
            if (new List<char> { 'A', 'D', 'H', 'M', 'N', 'R', 'V', 'W' }.Contains(Symbol))
                Bases.Add('A');
            if (new List<char> { 'B', 'C', 'H', 'M', 'N', 'S', 'V', 'Y' }.Contains(Symbol))
                Bases.Add('C');
            if (new List<char> { 'B', 'D', 'G', 'K', 'N', 'R', 'S', 'V' }.Contains(Symbol))
                Bases.Add('G');
            if (new List<char> { 'B', 'D', 'H', 'K', 'N', 'U', 'W', 'Y' }.Contains(Symbol))
                Bases.Add('U');
            if (Symbol == 'T')
                Bases.Add('T');
            
            if (Bases.Count == 0)
                throw new RibosoftException(R_STATUS.R_INVALID_NUCLEOTIDE, String.Format("Invalid nucleotide base {0} was provided.", Symbol));
        }

        public char GetComplement()
        {
            switch (Symbol)
            {
                case 'A':
                    return 'U';
                case 'U':
                    return 'A';
                case 'G':
                    return 'C';
                case 'C':
                    return 'G';
                default:
                    throw new RibosoftException(R_STATUS.R_INVALID_NUCLEOTIDE, String.Format("Cannot get complement of invalid symbol {0}", Symbol));
            }
        }

        // Allows GU Pairing but ONLY defined on the ribozyme... The software will not force a GU pair, that is up to the user
        public char[] GetSpecialComplements()
        {
            switch (Symbol)
            {
                case 'A':
                    return new char[] { 'U' };
                case 'U':
                    return new char[] { 'A', 'G' };
                case 'G':
                    return new char[] { 'C', 'U' };
                case 'C':
                    return new char[] { 'G' };
                default:
                    throw new RibosoftException(R_STATUS.R_INVALID_NUCLEOTIDE, String.Format("Cannot get complement of invalid symbol {0}", Symbol));
            }
        }
    }
}

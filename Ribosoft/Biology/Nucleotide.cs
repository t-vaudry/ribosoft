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
            switch (Symbol)
            {
                case 'A':
                    Bases.Add('A');
                    IsBase = true;
                    break;
                case 'C':
                    Bases.Add('C');
                    IsBase = true;
                    break;
                case 'G':
                    Bases.Add('G');
                    IsBase = true;
                    break;
                case 'U':
                    Bases.Add('U');
                    IsBase = true;
                    break;
                case 'T':
                    Bases.Add('T');
                    IsBase = true;
                    break;
                case 'W':
                    Bases.Add('A');
                    Bases.Add('U');
                    break;
                case 'S':
                    Bases.Add('C');
                    Bases.Add('G');
                    break;
                case 'M':
                    Bases.Add('A');
                    Bases.Add('C');
                    break;
                case 'K':
                    Bases.Add('G');
                    Bases.Add('U');
                    break;
                case 'R':
                    Bases.Add('A');
                    Bases.Add('G');
                    break;
                case 'Y':
                    Bases.Add('C');
                    Bases.Add('U');
                    break;
                case 'B':
                    Bases.Add('C');
                    Bases.Add('G');
                    Bases.Add('U');
                    break;
                case 'D':
                    Bases.Add('A');
                    Bases.Add('G');
                    Bases.Add('U');
                    break;
                case 'H':
                    Bases.Add('A');
                    Bases.Add('C');
                    Bases.Add('U');
                    break;
                case 'V':
                    Bases.Add('A');
                    Bases.Add('C');
                    Bases.Add('G');
                    break;
                case 'N':
                    Bases.Add('A');
                    Bases.Add('C');
                    Bases.Add('G');
                    Bases.Add('U');
                    break;
                case '-': //For nucleotides that will be set later
                    break;
                default:
                    throw new RibosoftException(R_STATUS.R_INVALID_NUCLEOTIDE, String.Format("Invalid nucleotide base {0} was provided.", Symbol));
            }
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

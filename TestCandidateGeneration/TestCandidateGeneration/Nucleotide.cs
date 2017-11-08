using System;
using System.Collections.Generic;
using System.Text;

namespace TestCandidateGeneration
{
    class Nucleotide
    {
        public char mSymbol = 'X';
        public List<char> mBases;
        public bool mIsBase = false;

        public Nucleotide()
        {

        }

        public Nucleotide(Nucleotide other)
        {
            mSymbol = other.mSymbol;
            mBases = other.mBases;
            mIsBase = other.mIsBase;
        }

        public Nucleotide(char symbol)
        {
            mSymbol = Char.ToUpper(symbol);
            mBases = new List<char>();
            switch(mSymbol)
            {
                case 'A':
                    mBases.Add('A');
                    mIsBase = true;
                    break;
                case 'C':
                    mBases.Add('C');
                    mIsBase = true;
                    break;
                case 'G':
                    mBases.Add('G');
                    mIsBase = true;
                    break;
                case 'U':
                    mBases.Add('U');
                    mIsBase = true;
                    break;
                case 'T':
                    mBases.Add('T');
                    mIsBase = true;
                    break;
                case 'W':
                    mBases.Add('A');
                    mBases.Add('U');
                    break;
                case 'S':
                    mBases.Add('C');
                    mBases.Add('G');
                    break;
                case 'M':
                    mBases.Add('A');
                    mBases.Add('C');
                    break;
                case 'K':
                    mBases.Add('G');
                    mBases.Add('U');
                    break;
                case 'R':
                    mBases.Add('A');
                    mBases.Add('G');
                    break;
                case 'Y':
                    mBases.Add('C');
                    mBases.Add('U');
                    break;
                case 'B':
                    mBases.Add('C');
                    mBases.Add('G');
                    mBases.Add('U');
                    break;
                case 'D':
                    mBases.Add('A');
                    mBases.Add('G');
                    mBases.Add('U');
                    break;
                case 'H':
                    mBases.Add('A');
                    mBases.Add('C');
                    mBases.Add('U');
                    break;
                case 'V':
                    mBases.Add('A');
                    mBases.Add('C');
                    mBases.Add('G');
                    break;
                case 'N':
                    mBases.Add('A');
                    mBases.Add('C');
                    mBases.Add('G');
                    mBases.Add('U');
                    break;
                default:
                    Console.WriteLine(String.Format("Nucleotide base {0} was provided.", mSymbol));
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ribosoft.Biology
{
    /*! \class Sequence
     * \brief Object class for the list of nucleotides in the sequence
     */
    public class Sequence
    {
        /*! \property Nucleotides
         * \brief List of nucleotides in the sequence
         */
        public List<Nucleotide> Nucleotides { get; set; }

        /*!
         * \brief Default constructor
         */
        public Sequence()
        {
            Nucleotides = new List<Nucleotide>();
        }

        /*!
         * \brief Constructor
         * \param capacity Sets the length of the sequence
         */
        public Sequence(int capacity)
        {
            Nucleotides = new List<Nucleotide>(capacity);
        }

        /*!
         * \brief Copy constructor
         * \param otherSequence Sequence to copy
         */
        public Sequence(Sequence otherSequence)
        {
            Nucleotides = new List<Nucleotide>(otherSequence.Nucleotides);
        }

        /*!
         * \brief Constructor
         * \param sequence String of bases to set as sequence of nucleotides
         */
        public Sequence(string sequence) : this()
        {
            Nucleotides = sequence.Select(c => new Nucleotide(c)).ToList();
        }

        /*! \fn GetString
         * \brief Gets the sequence as a string
         * \return Sequence string
         */
        public String GetString()
        {
            StringBuilder sb = new System.Text.StringBuilder();

            foreach (Nucleotide nucleotide in Nucleotides)
                sb.Append(nucleotide.Symbol);

            return sb.ToString();
        }

        /*! \fn GetCharAt
         * \brief Gets base at index
         * \param index Index for base
         * \return Symbol character
         */
        public char GetCharAt(int index)
        {
            return Nucleotides[index].Symbol;
        }

        /*! \fn GetComplement
         * \brief Gets the complement of the current sequence
         * \return Complement string
         */
        public String GetComplement()
        {
            StringBuilder complement = new System.Text.StringBuilder();

            foreach (Nucleotide n in Nucleotides)
                complement.Append(n.GetComplement());

            return complement.ToString();
        }

        /*! \fn GetLength
         * \brief Gets length of sequence
         * \return Sequence length
         */
        public int GetLength()
        {
            return Nucleotides.Count;
        }

        /*! \fn Insert
         * \brief Inserts a nucleotide at a given index
         * \param idx Index to insert nucleotide
         * \param nucleotide Nucleotide to insert
         */
        public void Insert(int idx, Nucleotide nucleotide)
        {
            Nucleotides.Insert(idx, nucleotide);
        }
    }
}

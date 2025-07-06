using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Ribosoft.ValidationAttributes
{
    /*! \class RepeatNotationsAttribute
     * \brief Validation class for repeat notations in a structure
     */
    public class RepeatNotationsAttribute : ValidationAttribute
    {
        /*! \property _maxRepeats
         * \brief Value of maximum repeat notations in RNA sequence
         */
        private int _maxRepeats;

        /*! \property _repeatNotations
         * \brief List of acceptable repeat notations
         */
        private char[] _repeatNotations = {'a', 'c', 'g', 'u', 'r', 'y', 'k', 'm', 's', 'w', 'b', 'd', 'h', 'v', 'n'};

        /*! \fn RepeatNotationsAttribute
         * \brief Default constructor
         * \param MaxRepeats Maximum number of repeat notations
         */
        public RepeatNotationsAttribute(int MaxRepeats)
        {
            _maxRepeats = MaxRepeats;
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            int count = 0;
            string sequence = value.ToString() ?? string.Empty;
            
            foreach (char c in sequence)
            {
                if (_repeatNotations.Contains(c))
                {
                    count++;
                }
            }

            return (count <= _maxRepeats);
        }

        /*! \fn FormatErrorMessage
         * \brief Function to format error message
         * \param name Name of invalidated value
         * \return Formatted error message
         */
        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, "Amount of repeat notations cannot exceed " + _maxRepeats, name);
        }
    }
}
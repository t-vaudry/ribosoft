using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ribosoft.ValidationAttributes
{
    /*! \class NucleotideAttribute
     * \brief Validation class for the attributes of nucleotides
     */
    public class NucleotideAttribute : ValidationAttribute
    {
        /*! \property _pattern
         * \brief Regex pattern for validation
         */
        private string _pattern;

        /*! \fn NucleotideAttribute
         * \brief Default constructor
         */
        public NucleotideAttribute()
        {
            _pattern = @"^['A','C','G','U','T','R','Y','K','M','S','W','B','D','H','V','N','a','c','g','u','t','r','y','k','m','s','w','b','d','h','v','n']+$";
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
        public override bool IsValid(object value)
        {
            Regex r = new Regex(_pattern);
            Match m = r.Match(value.ToString());

            return m.Success;
        }

        /*! \fn FormatErrorMessage
         * \brief Function to format error message
         * \param name Name of invalidated value
         * \return Formatted error message
         */
        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, name+" must only contain the following upper or lower case values: A, C, G, U, T, R, Y, K, M, S, W, B, D, H, V, N", name);
        }
    }
}
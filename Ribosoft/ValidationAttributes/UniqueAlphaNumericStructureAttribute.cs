using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//! \namespace Ribosoft.ValidationAttributes
namespace Ribosoft.ValidationAttributes
{
    /*! \class UniqueAlphaNumericStructureAttribute
     * \brief UniqueAlphaNumericStructureAttribute validation class
     * This class is used to validate the alphanumeric structure of a ribozyme.
     */
    public class UniqueAlphaNumericStructureAttribute : ValidationAttribute
    {
        /*! \property _isValid
         * \brief Boolean of current state of attribute
         */
        private bool _isValid;

        /*! \property _errorMessage
         * \brief Error message
         */
        private string _errorMessage;

        /*! \fn UniqueAlphaNumericStructureAttribute
         * \brief Default constructor
         */
        public UniqueAlphaNumericStructureAttribute()
        {
            _isValid = true;
            _errorMessage = "Alphanumerics within the structure must only occur once";
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            _isValid = true;
            string structure = value.ToString() ?? string.Empty;
            var validInputs = new List<char>(new char[] {'.', '(', ')', '[', ']'});
            var occured = new List<char>();

            foreach (char c in structure)
            {
                if (Char.IsLower(c) || Char.IsDigit(c))
                {
                    if (occured.Contains(c))
                    {
                        _isValid = false;
                    }
                    else
                    {
                        occured.Add(c);
                    }
                }
                else if (!validInputs.Contains(c))
                {
                    _isValid = false;
                }
            }

            return _isValid;
        }

        /*! \fn FormatErrorMessage
         * \brief Function to format error message
         * \param name Name of invalidated value
         * \return Formatted error message
         */
        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, _errorMessage, name);
        }
    }
}
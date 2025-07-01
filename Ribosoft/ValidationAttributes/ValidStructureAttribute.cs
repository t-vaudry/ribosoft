using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ribosoft.ValidationAttributes
{
    /*! \class ValidStructureAttribute
     * \brief Validation class for the structure bonds in a structure
     */
    public class ValidStructureAttribute : ValidationAttribute
    {
        /*! \property _isValid
         * \brief Boolean of current state of attribute
         */
        private bool _isValid;

        /*! \property _errorMessage
         * \brief Error message
         */
        private string _errorMessage;

        /*! \fn ValidStructureAttribute
         * \brief Default constructor
         */
        public ValidStructureAttribute()
        {
            _isValid = true;
            _errorMessage = "Invalid structure format, ensure all bonds and pseudo knots have matching closing symbols";
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
        public override bool IsValid(object? value)
        {
            // Reset boolean and error message for second pass of validation
            _isValid = true;
            _errorMessage = "";

            if (value == null) return false;
            
            string structure = value.ToString() ?? string.Empty;        
            uint OpenDoubleBondCount = 0;       // (
            uint OpenPseudoKnotCount = 0;       // [
            
            // Match double bond and pseudo knot pairs
            foreach (char c in structure)
            {
                switch (c)
                {
                    case '(':
                        OpenDoubleBondCount++;
                        break;
                    case ')':
                        if (OpenDoubleBondCount > 0)
                        {
                            OpenDoubleBondCount--;
                        }
                        else
                        {
                            _isValid = false;
                        }
                        break;
                    case '[':
                        OpenPseudoKnotCount++;
                        break;
                    case ']':
                        if (OpenPseudoKnotCount > 0)
                        {
                            OpenPseudoKnotCount--;
                        }
                        else
                        {
                            _isValid = false;
                        }
                        break;
                    default:
                        break;
                }
            }

            // Verify there are no unclosed double bonds
            if (OpenDoubleBondCount != 0)
            {
                _isValid = false;
            }

            // Verify there are no unclosed pseudo knots
            if (OpenPseudoKnotCount != 0)
            {
                _isValid = false;
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
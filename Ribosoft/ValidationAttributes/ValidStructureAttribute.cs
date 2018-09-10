using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ribosoft.ValidationAttributes
{
    public class ValidStructureAttribute : ValidationAttribute
    {
        private bool _isValid;
        private string _errorMessage;
        public ValidStructureAttribute()
        {
            _isValid = true;
            _errorMessage = "Invalid structure format, ensure all bonds and pseudo knots have matching closing symbols";
        }

        public override bool IsValid(object value)
        {
            // Reset boolean and error message for second pass of validation
            _isValid = true;
            _errorMessage = "";

            string structure = value.ToString();        
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

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, _errorMessage, name);
        }
    }
}
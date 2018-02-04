using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class ValidStructureAttribute : ValidationAttribute
{
    private bool _isValid;
    private string _ErrorMessage;
    public ValidStructureAttribute()
    {
        _isValid = true;
        _ErrorMessage = "";
    }

    public override bool IsValid(object value)
    {
        // Reset boolean and error message for second pass of validation
        _isValid = true;
        _ErrorMessage = "";

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
                        _ErrorMessage += "Structure contains closing double bond without matching opening bond\n";
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
                        _ErrorMessage += "Structure contains closing pseudo knot without matching opening pseudo knot\n";
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
            _ErrorMessage += "Mismatching double bond pairs\n";
        }

        // Verify there are no unclosed pseudo knots
        if (OpenPseudoKnotCount != 0)
        {
            _isValid = false;
            _ErrorMessage += "Mismatching pseudo knot pairs\n";
        }

        return _isValid;
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, _ErrorMessage, name);
    }
}
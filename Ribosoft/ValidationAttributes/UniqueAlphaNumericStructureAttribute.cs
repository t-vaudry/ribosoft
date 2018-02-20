using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class UniqueAlphaNumericStructureAttribute : ValidationAttribute
{
    private bool _isValid;
    private string _errorMessage;

    public UniqueAlphaNumericStructureAttribute()
    {
        _isValid = true;
        _errorMessage = "Alphanumerics within the structure must only occure once";
    }

    public override bool IsValid(object value)
    {
        string structure = value.ToString();
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

     public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, _errorMessage, name);
    }
}
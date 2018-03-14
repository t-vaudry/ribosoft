using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

public class OpenReadingFrameAttribute : ValidationAttribute
{
    private string _errorMessage;

    public OpenReadingFrameAttribute()
    {
        _errorMessage = "Index must be a positive integer";
    }

    public override bool IsValid(object value)
    {
        int start = value as int? ?? -1;

        return start >= 0;
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, _errorMessage, name);
    }
}
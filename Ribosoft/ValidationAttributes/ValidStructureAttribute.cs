using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
public class ValidStructureAttribute : ValidationAttribute
{
    private string _ErrorMessage;
    public ValidStructureAttribute()
    {
        _ErrorMessage = "Invalid structure format";
    }

    public override bool IsValid(object value)
    {
        // TODO: Validate

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, _ErrorMessage, name);
    }
}
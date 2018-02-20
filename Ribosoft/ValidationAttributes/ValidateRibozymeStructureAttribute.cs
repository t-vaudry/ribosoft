using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Ribosoft.Models;
public class ValidateRibozymeStructureAttribute : ValidationAttribute
{
    private bool _isValid;
    string _errorMessage;
    public ValidateRibozymeStructureAttribute()
    {
        _isValid = true;
        _errorMessage = "";
    }

    public override bool IsValid(object value)
    {
        RibozymeStructure model = (RibozymeStructure) value;
        // Reset boolean and error message for second pass of validation
        _isValid = true;
        _errorMessage = "";

        // Validate Cutsite is within Substrate Template
        if (model.Cutsite > model.SubstrateTemplate.Length)
        {
            _isValid = false;
            _errorMessage += "Cutsite position must not exceed the Substrate Template length\n";
        }

        // Validate Sequence length matches Structure length
        if (model.Sequence.Length != model.Structure.Length)
        {
            _isValid = false;
            _errorMessage += "Sequence template and structure must contain the same amount of elements\n";
        }

        // Validate Substrate Template length matches Substrate Structure length
        if (model.SubstrateTemplate.Length != model.SubstrateStructure.Length)
        {
            _isValid = false;
            _errorMessage += "Substrate template and structure must contain the same amount of elements\n";
        }

        // Validate Structure and Substrate Structure alphanums are equivalent
        if (!matchingAlphaNumerics(model.Structure, model.SubstrateStructure))
        {
            _isValid = false;
            _errorMessage += "Alphanumerics within Sequence Structure and Substrate Structure do not match";
        }

        return _isValid;
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, _errorMessage, name);
    }

    public bool matchingAlphaNumerics(string sequenceStructure, string substrateStructure)
    {
        string sequenceStructureAlphanumeric = "";
        string substrateStructureAlphanumeric = "";
        Regex r = new Regex(@"[a-z0-9]");

        foreach (char c in sequenceStructure) 
        {
            Match m = r.Match(c.ToString());
            
            if (m.Success)
            {
                sequenceStructureAlphanumeric += c;
            }
        }

        foreach (char c in substrateStructure) 
        {
            Match m = r.Match(c.ToString());
            
            if (m.Success)
            {
                substrateStructureAlphanumeric += c;
            }
        }

        return sequenceStructureAlphanumeric == substrateStructureAlphanumeric;
    }
}
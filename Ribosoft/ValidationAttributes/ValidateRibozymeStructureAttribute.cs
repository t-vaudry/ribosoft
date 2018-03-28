using System;
using System.Linq;
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
        _errorMessage = "Invalid input, ensure sequence and substrates match their structures";
    }

    public override bool IsValid(object value)
    {
        RibozymeStructure model = value as RibozymeStructure;
        // Reset boolean and error message for second pass of validation
        _isValid = true;

        // Validate Cutsite is within Substrate Template
        if (model.Cutsite > model.SubstrateTemplate.Length)
        {
            _isValid = false;
        }

        // Validate Sequence length matches Structure length
        if (model.Sequence.Length != model.Structure.Length)
        {
            _isValid = false;
        }

        // Validate Substrate Template length matches Substrate Structure length
        if (model.SubstrateTemplate.Length != model.SubstrateStructure.Length)
        {
            _isValid = false;
        }

        // Validate Structure and Substrate Structure alphanums are equivalent
        if (!matchingAlphaNumerics(model.Structure, model.SubstrateStructure))
        {
            _isValid = false;
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

        // Get all alphanumerics in sequence
        foreach (char c in sequenceStructure) 
        {
            Match m = r.Match(c.ToString());
            
            if (m.Success)
            {
                sequenceStructureAlphanumeric += c;
            }
        }

        // Get all alphanumerics in substrate
        foreach (char c in substrateStructure) 
        {
            Match m = r.Match(c.ToString());
            
            if (m.Success)
            {
                substrateStructureAlphanumeric += c;
            }
        }

        // Sort alphanumeric strings
        sequenceStructureAlphanumeric = String.Concat(sequenceStructureAlphanumeric.OrderBy(c => c));
        substrateStructureAlphanumeric = String.Concat(substrateStructureAlphanumeric.OrderBy(c => c));

        return sequenceStructureAlphanumeric == substrateStructureAlphanumeric;
    }
}
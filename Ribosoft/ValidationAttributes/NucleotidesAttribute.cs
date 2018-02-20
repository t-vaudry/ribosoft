using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class NucleotideAttribute : ValidationAttribute
{
    private string _pattern;
    public NucleotideAttribute()
    {
        _pattern = @"^['A','C','G','U','R','Y','K','M','S','W','B','D','H','V','N','n']+$";
    }

    public override bool IsValid(object value)
    {
        Regex r = new Regex(_pattern);
        Match m = r.Match(value.ToString());

        return m.Success;
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, name+" must only contain the following characters: A, C, G, U, R, Y, K, M, S, W, B, D, H, V, N, n", name);
    }
}
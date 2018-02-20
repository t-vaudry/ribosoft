using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Linq;
public class RepeatNotationsAttribute : ValidationAttribute
{
    private int _maxRepeats;
    private char[] _repeatNotations = {'a', 'c', 'g', 'u', 'r', 'y', 'k', 'm', 's', 'w', 'b', 'd', 'h', 'v', 'n'};
    public RepeatNotationsAttribute(int MaxRepeats)
    {
        _maxRepeats = MaxRepeats;
    }

    public override bool IsValid(object value)
    {
        int count = 0;
        string sequence = value.ToString();
        
        foreach (char c in sequence)
        {
            if (_repeatNotations.Contains(c))
            {
                count++;
            }
        }

        return (count <= _maxRepeats);
    }

    public override string FormatErrorMessage(string name)
    {
        return String.Format(CultureInfo.CurrentCulture, "Repeat n notation cannot exced 8", name);
    }
}
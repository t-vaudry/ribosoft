using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ribosoft.ValidationAttributes
{
    public class NucleotideAttribute : ValidationAttribute
    {
        private string _pattern;
        public NucleotideAttribute()
        {
            _pattern = @"^['A','C','G','U','T','R','Y','K','M','S','W','B','D','H','V','N','a','c','g','u','t','r','y','k','m','s','w','b','d','h','v','n']+$";
        }

        public override bool IsValid(object value)
        {
            Regex r = new Regex(_pattern);
            Match m = r.Match(value.ToString());

            return m.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, name+" must only contain the following upper or lower case values: A, C, G, U, T, R, Y, K, M, S, W, B, D, H, V, N", name);
        }
    }
}
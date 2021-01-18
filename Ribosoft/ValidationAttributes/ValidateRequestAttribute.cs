using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using Ribosoft.Models;
using Ribosoft.Models.RequestViewModels;

namespace Ribosoft.ValidationAttributes
{
    public class ValidateRequestAttribute : ValidationAttribute
    {
        private bool _isValid;
        private string _errorMessage;

        public ValidateRequestAttribute()
        {
            _isValid = true;
            _errorMessage = "Invalid start and end index, verify end position is after start and within the sequence";
        }

        public override bool IsValid(object value)
        {
            RequestViewModel model = value as RequestViewModel;
            _isValid = true;

            if (model.OpenReadingFrameEnd < model.OpenReadingFrameStart) {
                _isValid = false;
            }

            if (model.OpenReadingFrameEnd > model.InputSequence.Length || 
                model.OpenReadingFrameStart > model.InputSequence.Length) {
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
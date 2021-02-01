using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using Ribosoft.Models;
using Ribosoft.Models.RequestViewModels;

namespace Ribosoft.ValidationAttributes
{
    /*! \class ValidateRequestAttribute
     * \brief Validation class for the attributes of a request
     */
    public class ValidateRequestAttribute : ValidationAttribute
    {
        /*! \property _isValid
         * \brief Boolean of current state of attribute
         */
        private bool _isValid;

        /*! \property _errorMessage
         * \brief Error message
         */
        private string _errorMessage;

        /*! \fn ValidateRequestAttribute
         * \brief Default constructor
         */
        public ValidateRequestAttribute()
        {
            _isValid = true;
            _errorMessage = "Invalid start and end index, verify end position is after start and within the sequence";
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
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

        /*! \fn FormatErrorMessage
         * \brief Function to format error message
         * \param name Name of invalidated value
         * \return Formatted error message
         */
        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, _errorMessage, name);
        }
    }
}
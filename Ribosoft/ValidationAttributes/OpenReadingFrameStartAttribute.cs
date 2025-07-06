using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

//! \namespace Ribosoft.ValidationAttributes
namespace Ribosoft.ValidationAttributes
{
    /*! \class OpenReadingFrameAttribute
     * \brief OpenReadingFrameAttribute validation class
     * This class is used to validate the open reading frame attribute; it must be a positive integer.
     */
    public class OpenReadingFrameAttribute : ValidationAttribute
    {
        /*! \property _errorMessage
         * \brief Regex pattern for validation
         */
        private string _errorMessage;

        /*! \fn OpenReadingFrameAttribute
         * \brief Default constructor
         */
        public OpenReadingFrameAttribute()
        {
            _errorMessage = "Index must be a positive integer";
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
        public override bool IsValid(object? value)
        {
            int start = value as int? ?? -1;

            return start >= 0;
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
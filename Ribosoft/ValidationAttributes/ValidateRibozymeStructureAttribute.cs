using System;
using System.Linq;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Ribosoft.Models;
using Ribosoft.Models.RibozymeViewModel;

namespace Ribosoft.ValidationAttributes
{
    /*! \class ValidateRibozymeStructureAttribute
     * \brief Validation class for the attributes of the ribozyme structure
     */
    public class ValidateRibozymeStructureAttribute : ValidationAttribute
    {
        /*! \property _isValid
         * \brief Boolean of current state of attribute
         */
        private bool _isValid;

        /*! \property _errorMessage
         * \brief Error message
         */
        private string _errorMessage;

        /*! \fn ValidateRibozymeStructureAttribute
         * \brief Default constructor
         */
        public ValidateRibozymeStructureAttribute()
        {
            _isValid = true;
            _errorMessage = "Invalid input, ensure sequence and substrates match their structures";
        }

        /*! \fn IsValid
         * \brief Validation function
         * \param value Value to validate
         * \return Boolean result of check
         */
        public override bool IsValid(object value)
        {
            // Reset boolean and error message for second pass of validation
            _isValid = true;

            if (value is RibozymeStructure ribozymeStructure)
            {
                _isValid = IsValidRibozymeStructure(ribozymeStructure);
            }

            if (value is RibozymeCreateViewModel ribozymeCreateViewModel)
            {
                _isValid = IsValidRibozymeCreateViewModel(ribozymeCreateViewModel);
            }

            return _isValid;
        }

        /*! \fn IsValidRibozymeStructure
         * \brief Validation function for ribozyme structure
         * \param model Ribozyme structure model
         * \return Boolean result of check
         */
        private bool IsValidRibozymeStructure(RibozymeStructure model)
        {
            // Validate Cutsite is within Substrate Template
            if (model.Cutsite > model.SubstrateTemplate.Length)
            {
            return false;
            }

            // Validate Sequence length matches Structure length
            if (model.Sequence.Length != model.Structure.Length)
            {
            return false;
            }

            // Validate Substrate Template length matches Substrate Structure length
            if (model.SubstrateTemplate.Length != model.SubstrateStructure.Length)
            {
            return false;
            }

            // Validate Structure and Substrate Structure alphanums are equivalent
            if (!matchingAlphaNumerics(model.Structure, model.SubstrateStructure))
            {
            return false;
            }

        return true;
        }

        /*! \fn IsValidRibozymeCreateViewModel
         * \brief Validation function for ribozyme structure creation
         * \param model Ribozyme structure create model
         * \return Boolean result of check
         */
        private bool IsValidRibozymeCreateViewModel(RibozymeCreateViewModel model)
        {
            foreach (var structure in model.RibozymeStructures)
            {
                if(!IsValidRibozymeStructure(structure))
                {
                    return false;
                }    
            }

            return true;
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

        /*! \fn matchingAlphaNumerics
         * \brief Helper function to check for matching target regions
         * \param sequenceStructure Ribozyme structure
         * \param substrateStructure Substrate structure
         * \return Boolean result of the check
         */
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
}
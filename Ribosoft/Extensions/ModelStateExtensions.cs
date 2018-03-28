using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ribosoft.Extensions
{
    public static class ModelStateExtensions
    {
        public static string InvalidFieldClass(this ModelStateDictionary modelStateDictionary, string property)
        {
            return modelStateDictionary.GetFieldValidationState(property) == ModelValidationState.Invalid ? "is-invalid" : "";
        }
    }
}
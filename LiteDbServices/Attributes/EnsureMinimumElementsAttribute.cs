using System.Collections; 
using System.ComponentModel.DataAnnotations; 

namespace LiteDbServices.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EnsureMinimumElementsAttribute : ValidationAttribute
    {
        private readonly int _minElements;

        public EnsureMinimumElementsAttribute(int minElements)
        {
            _minElements = minElements;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection collection && collection.Count < _minElements)
            {
                return new ValidationResult($"The {validationContext.DisplayName} must have at least {_minElements} element(s).");
            }

            return ValidationResult.Success;
        }
    }
}

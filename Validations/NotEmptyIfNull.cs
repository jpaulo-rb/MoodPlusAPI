using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Validations
{
    public class NotEmptyIfNotNull : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string s && s.Length == 0)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} não pode ser vazio");
            }
            return ValidationResult.Success;
        }
    }
}
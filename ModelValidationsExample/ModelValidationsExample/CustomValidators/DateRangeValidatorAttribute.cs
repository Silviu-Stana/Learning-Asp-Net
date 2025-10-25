using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModelValidationsExample.CustomValidators
{
    public class DateRangeValidatorAttribute : ValidationAttribute
    {
        public string OtherPropertyName { get; set; }
        public string DefaultErrorMessage { get; set; } = "FromDate, should be on a date before ToDate";
        public DateRangeValidatorAttribute(string otherPropertyName) { OtherPropertyName = otherPropertyName; }


        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime to_date = Convert.ToDateTime(value);

                PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(OtherPropertyName);
                DateTime from_date = Convert.ToDateTime(otherProperty.GetValue(validationContext.ObjectInstance));

                if (from_date > to_date) return new ValidationResult(ErrorMessage ?? DefaultErrorMessage, new string[] {OtherPropertyName, validationContext.MemberName});

                return ValidationResult.Success;
            }
        }
    }
}

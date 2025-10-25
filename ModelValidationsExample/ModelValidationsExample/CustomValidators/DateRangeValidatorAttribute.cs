using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModelValidationsExample.CustomValidators
{
    public class DateRangeValidatorAttribute : ValidationAttribute
    {
        public string FromDate { get; set; }
        public string DefaultErrorMessage { get; set; } = "'From Date' should be older than 'To Date'";
        public DateRangeValidatorAttribute(string fromDate) { FromDate = fromDate; }


        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime to_date = Convert.ToDateTime(value);

                //Fetch  FromDate  using reflection
                PropertyInfo? otherProperty = validationContext.ObjectType.GetProperty(FromDate);
                DateTime from_date = Convert.ToDateTime(otherProperty.GetValue(validationContext.ObjectInstance));

                if (from_date > to_date) return new ValidationResult(ErrorMessage ?? DefaultErrorMessage/*, new string[] {FromDate, validationContext.MemberName}*/);

                else return ValidationResult.Success;
            }

            return null;
        }
    }
}

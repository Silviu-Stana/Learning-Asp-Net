using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelValidationsExample.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace ModelValidationsExample.Models
{
    public class Person : IValidatableObject
    {
        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Person Name")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "{0} must be from {2} to {1} characters long.")]
        [RegularExpression("^[A-Za-z .]+$", ErrorMessage = "{0} must contain only alphabet letters.")]
        public string? PersonName { get; set; }
        [EmailAddress] [Required] public string? Email { get; set; }
        [Phone] public string? Phone { get; set; }
        [Required(ErrorMessage ="{0} is required")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [Compare("Password", ErrorMessage = "Your passwords do not match!")]
        public string? ConfirmPassword { get; set; }

        [Range(0,999.99, ErrorMessage="{0} should be between ${1} and ${2}")]
        public double? Price { get; set; }

        //[BindNever]
        public DateTime? DateOfBirth { get; set; }
        public int? Age{  get; set; }

        public DateTime? FromDate {  get; set; }
        [DateRangeValidator("FromDate")]
        public DateTime? ToDate {  get; set; }

        public List<string?> Tags { get; set; } = [];
        public override string ToString()
        {
            return $"Name: {PersonName}, Email: {Email}, Phone: {Phone}, Password: {Password}, Price: {Price}";
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth.HasValue == false && Age.HasValue == false)
            {
                yield return new ValidationResult("Either Age or Date of Birth should be supplied.", new[]  { nameof(Age) });
            }
        }
    }
}

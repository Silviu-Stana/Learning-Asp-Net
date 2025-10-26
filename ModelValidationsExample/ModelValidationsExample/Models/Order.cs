using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelValidationsExample.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace ModelValidationsExample.Models
{
    public class Order
    {
        [BindNever]
        public int? OrderNo { get; set; }
        [Required]
        [MinimumYearValidator(2000, ErrorMessage = "Order date should be after the year 2000.")]
        public DateTime OrderDate { get; set; }
        [Required]
        [RegularExpression("^[0-9]+$")]
        public double InvoicePrice { get; set; }
        [Required] [MinLength(1)]
        public List<Product> Products { get; set; } = [];


    }
}

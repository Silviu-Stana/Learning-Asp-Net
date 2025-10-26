using System.ComponentModel.DataAnnotations;

namespace ModelValidationsExample.Models
{
    public class Product
    {
        [Required]
        [RegularExpression("^[0-9]+$")]
        public int ProductCode { get; set; }
        [Required]
        [RegularExpression(@"^-?\d+(\.\d+)?$")]
        public double Price { get; set; }
        [Required]
        [RegularExpression("^[0-9]+$")]
        public int Quantity { get; set; }
    }
}

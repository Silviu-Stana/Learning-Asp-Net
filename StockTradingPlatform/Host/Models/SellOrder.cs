using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class SellOrder
    {
        [Key]
        [Required] public Guid SellOrderID { get; set; }
        [Required][MaxLength(10)] public string? StockSymbol { get; set; }
        [Required][MaxLength(30)] public string? StockName { get; set; }
        public DateTime OrderDate { get; set; }
        [Required] [Range(1, 100000)] public uint Quantity { get; set; }
        [Required] [Range(1, 10000)] public double Price { get; set; }

    }
}

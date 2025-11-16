using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class SellOrderRequest
    {
        [Required] public string? StockSymbol { get; set; }
        [Required] public string? StockName { get; set; }
        public DateTime OrderDate { get; set; }
        [Range(1, 100000)] public uint Quantity { get; set; }
        [Range(1, 10000)] public double Price { get; set; }
    }
}

using Models.Models;
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

        /// <summary>
        /// Convert <see cref="SellOrderRequest"/> DTO to new <see cref="SellOrder"/> object.
        /// </summary>
        /// <returns><see cref="SellOrder"/> object</returns>
        public SellOrder ConvertToSellOrder()
        {
            return new SellOrder()
            {
                StockSymbol = StockSymbol,
                StockName = StockName,
                Price = Price,
                OrderDate = OrderDate,
                SellOrderID = Guid.NewGuid(),
                Quantity = Quantity
            };
        }
    }
}

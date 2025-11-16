using Models.Models;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class BuyOrderRequest
    {
        [Required] public string? StockSymbol { get; set; }
        [Required] public string? StockName { get; set; }
        public DateTime OrderDate { get; set; }
        [Range(1, 100000)] public uint Quantity { get; set; }
        [Range(1, 10000)] public double Price { get; set; }

        /// <summary>
        /// Convert <see cref="BuyOrderRequest"/> DTO to new <see cref="BuyOrder"/> object.
        /// </summary>
        /// <returns><see cref="BuyOrder"/> object</returns>
        public BuyOrder ConvertToBuyOrder()
        {
            return new BuyOrder()
            {
                StockSymbol = StockSymbol,
                StockName = StockName,
                Price = Price,
                OrderDate = OrderDate,
                BuyOrderID = Guid.NewGuid(),
                Quantity = Quantity
            };
        }
    }
}

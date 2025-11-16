using Models.Models;

namespace Models.DTO
{
    public class BuyOrderResponse
    {
        public Guid BuyOrderID { get; set; }

        public string? StockSymbol { get; set; }

        public string? StockName { get; set; }

        public DateTime OrderDate { get; set; }

        public uint Quantity { get; set; }

        public double Price { get; set; }

        public double TradeAmount { get; set; }

        public static BuyOrderResponse ConvertFrom(BuyOrder order)
        {
            return new BuyOrderResponse
            {
                BuyOrderID = order.BuyOrderID,
                StockSymbol = order.StockSymbol,
                StockName = order.StockName,
                Price = order.Price,
                OrderDate = order.OrderDate,
                Quantity = order.Quantity,
                TradeAmount = order.Price * order.Quantity
            };
        }
    }
}

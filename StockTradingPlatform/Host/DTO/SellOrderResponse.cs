using Models.Models;

namespace Models.DTO
{
    public class SellOrderResponse
    {
        public Guid SellOrderID { get; set; }
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public DateTime OrderDate { get; set; }
        public uint Quantity { get; set; }
        public double Price { get; set; }
        public double TradeAmount { get; set; }

        public static SellOrderResponse ConvertFrom(SellOrder order)
        {
            return new SellOrderResponse
            {
                SellOrderID = order.SellOrderID,
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

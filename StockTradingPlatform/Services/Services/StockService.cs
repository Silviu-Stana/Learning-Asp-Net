using Models.Interfaces;
using Models.Models;
using Models.DTO;

namespace Services.Services
{
    public class StockService : IStockService
    {
        private readonly List<BuyOrder> _buyOrders = [];
        private readonly List<SellOrder> _sellOrders = [];

        public async Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? request)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            if(request.Quantity<=0 || request.Quantity > 100000) throw new ArgumentException("Quantity must be between 1 and 100000.", nameof(request.Quantity));
            if (request.Price <= 0 || request.Price > 100000) throw new ArgumentException("Price must be a positive number, below 100000", nameof(request.Price));
            if (string.IsNullOrWhiteSpace(request.StockSymbol)) throw new ArgumentException("Stock symbol cannot be empty.", nameof(request.StockSymbol));

            await Task.Yield();

            var buyOrder = request.ConvertToBuyOrder();
            _buyOrders.Add(buyOrder);
            var response = BuyOrderResponse.ConvertFrom(buyOrder);

            return response;
        }
        public async Task<List<BuyOrderResponse>> GetBuyOrders()
        {
            List<BuyOrderResponse> buyOrders = [];

            await Task.Yield();

            foreach (var order in _buyOrders)
            {
                buyOrders.Add(BuyOrderResponse.ConvertFrom(order));
            }

            return buyOrders;
        }

        public async Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.Quantity <= 0 || request.Quantity > 100000) throw new ArgumentException("Quantity must be between 1 and 100000.", nameof(request.Quantity));
            if (request.Price <= 0 || request.Price > 100000) throw new ArgumentException("Price must be a positive number, below 100000", nameof(request.Price));
            if (string.IsNullOrWhiteSpace(request.StockSymbol)) throw new ArgumentException("Stock symbol cannot be empty.", nameof(request.StockSymbol));

            await Task.Yield();

            var sellOrder = request.ConvertToSellOrder();
            _sellOrders.Add(sellOrder);
            var response = SellOrderResponse.ConvertFrom(sellOrder);

            return response;
        }



        public async Task<List<SellOrderResponse>> GetSellOrders()
        {
            List<SellOrderResponse> sellOrders = [];

            await Task.Yield();

            foreach (var order in _sellOrders)
            {
                sellOrders.Add(SellOrderResponse.ConvertFrom(order));
            }

            return sellOrders;
        }
    }
}

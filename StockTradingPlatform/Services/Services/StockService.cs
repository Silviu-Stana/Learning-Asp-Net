using Models.Interfaces;
using Models.Models;
using Models.DTO;

namespace Services.Services
{
    public class StockService : IStockService
    {
        private readonly List<BuyOrder> _buyOrders = [];

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
            List<BuyOrderResponse> buyOrderResponses = [];

            await Task.Yield();

            foreach (var buyOrder in _buyOrders)
            {
                buyOrderResponses.Add(BuyOrderResponse.ConvertFrom(buyOrder));
            }

            return buyOrderResponses;
        }

        public Task<BuyOrderResponse> CreateSellOrder(SellOrderRequest? request)
        {
            throw new NotImplementedException();
        }



        public Task<List<SellOrderResponse>> GetSellOrders()
        {
            throw new NotImplementedException();
        }
    }
}

using Models.Interfaces;
using Models.Models;
using Models.DTO;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Services.Services
{
    public class StockService(StockMarketDbContext db) : IStockService
    {
        private readonly StockMarketDbContext _db = db;

        public async Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? request)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            if(request.Quantity<=0 || request.Quantity > 100000) throw new ArgumentException("Quantity must be between 1 and 100000.", nameof(request.Quantity));
            if (request.Price <= 0 || request.Price > 100000) throw new ArgumentException("Price must be a positive number, below 100000", nameof(request.Price));
            if (string.IsNullOrWhiteSpace(request.StockSymbol)) throw new ArgumentException("Stock symbol cannot be empty.", nameof(request.StockSymbol));

            var buyOrder = request.ConvertToBuyOrder();
            _db.BuyOrders.Add(buyOrder);
            await _db.SaveChangesAsync();

            var response = BuyOrderResponse.ConvertFrom(buyOrder);

            return response;
        }
        public async Task<List<BuyOrderResponse>> GetBuyOrders()
        {
            List<BuyOrder> buyOrders = await _db.BuyOrders.ToListAsync();

            return buyOrders.Select(BuyOrderResponse.ConvertFrom).ToList();
        }

        public async Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.Quantity <= 0 || request.Quantity > 100000) throw new ArgumentException("Quantity must be between 1 and 100000.", nameof(request.Quantity));
            if (request.Price <= 0 || request.Price > 100000) throw new ArgumentException("Price must be a positive number, below 100000", nameof(request.Price));
            if (string.IsNullOrWhiteSpace(request.StockSymbol)) throw new ArgumentException("Stock symbol cannot be empty.", nameof(request.StockSymbol));

            var sellOrder = request.ConvertToSellOrder();
            _db.SellOrders.Add(sellOrder);
            await _db.SaveChangesAsync();

            return SellOrderResponse.ConvertFrom(sellOrder);
        }



        public async Task<List<SellOrderResponse>> GetSellOrders()
        {
            List<SellOrder> sellOrders = await _db.SellOrders.ToListAsync();

            return sellOrders.Select(SellOrderResponse.ConvertFrom).ToList();
        }
    }
}

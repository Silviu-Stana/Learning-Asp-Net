using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Models.Interfaces;
using Services.Services;

namespace Tests
{
    public class StockServiceTests
    {
        private readonly IStockService _stockService;

        public StockServiceTests()
        {
            //_stockService = new StockService();
        }

        #region CreateBuyOrder
        [Fact]
        public async void CreateBuyOrder_RequestNull_ThrowsArgumentNull()
        {
            BuyOrderRequest? request = null;

            await Assert.ThrowsAsync<ArgumentNullException>(()=>_stockService.CreateBuyOrder(request));
        }

        [Theory] [InlineData(0)] [InlineData(100001)]
        public async void CreateBuyOrder_InvalidQuantity_ThrowsArgumentException(uint invalidQuantity)
        {
            BuyOrderRequest? request = new() {OrderDate=DateTime.Now, Price=100, Quantity= invalidQuantity, StockName="Microsoft Corp", StockSymbol="MSFT" };

            await Assert.ThrowsAsync<ArgumentException>(() => _stockService.CreateBuyOrder(request));
        }

        [Theory] [InlineData(0)] [InlineData(-5)] [InlineData(100001)]
        public async void CreateBuyOrder_InvalidPrice_ThrowsArgumentException(double invalidPrice)
        {
            BuyOrderRequest? request = new() { OrderDate = DateTime.Now, Price = invalidPrice, Quantity = 1, StockName = "Microsoft Corp", StockSymbol = "MSFT" };

            await Assert.ThrowsAsync<ArgumentException>(() => _stockService.CreateBuyOrder(request));
        }

        [Fact]
        public async void CreateBuyOrder_StockSymbolNull_ThrowsArgumentException()
        {
            BuyOrderRequest? request = new() { OrderDate = DateTime.Now, Price = 100, Quantity = 1, StockName = "Microsoft Corp", StockSymbol = null };

            await Assert.ThrowsAsync<ArgumentException>(() => _stockService.CreateBuyOrder(request));
        }

        [Fact]
        public async void CreateBuyOrder_Success()
        {
            BuyOrderRequest? request = new() { OrderDate = DateTime.Now, Price = 100, Quantity = 1, StockName = "Microsoft Corp", StockSymbol = "MSFT" };

            BuyOrderResponse buyOrder = await _stockService.CreateBuyOrder(request);

            //Generates valid order Guid:
            Assert.NotEqual(buyOrder.BuyOrderID, Guid.Empty);

            //Has the properties from the request:
            Assert.Equal(request.StockSymbol, buyOrder.StockSymbol);
            Assert.Equal(request.Quantity, buyOrder.Quantity);
            Assert.Equal(request.Price, buyOrder.Price);
        }

        #endregion
    }
}
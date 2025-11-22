using Models.Interfaces;
using Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models.DTO;

namespace Core.Controllers
{
    public class TradeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<TradingOptions> _tradingOptions;
        private readonly IFinnhubService _finnhubService;
        private readonly IStockService _stockService;

        public TradeController(IConfiguration configuration,
            IFinnhubService finnhubService, IStockService stockService, IOptions<TradingOptions> tradingOptions)
        {
            _configuration = configuration;
            _finnhubService = finnhubService;
            _tradingOptions = tradingOptions;
            _stockService = stockService;
        }

        [Route("/{stockSymbol?}")]
        public async Task<IActionResult> Index(string? stockSymbol)
        {
            if (stockSymbol == null) stockSymbol = _tradingOptions.Value.DefaultStockSymbol;

            Dictionary<string, object>? stockPrice = await _finnhubService.GetStockPriceQuote(stockSymbol);
            Dictionary<string, object>? stockProfile = await _finnhubService.GetCompanyProfile(stockSymbol);

            StockProfile stock = new()
            {
                StockSymbol = stockProfile!["ticker"].ToString(),
                StockName = stockProfile["name"].ToString(),
                //Price = Convert.ToDouble(stockPrice["c"].ToString()),
                Price = Convert.ToDouble(stockPrice!["c"].ToString()),
            };

            return View(stock);
        }

        [HttpPost("/trade/buy")]
        public async Task<IActionResult> BuyOrder(BuyOrderRequest buyOrder)
        {
            if (!ModelState.IsValid)
            {
                return View(buyOrder);
            }

            BuyOrderResponse order = await _stockService.CreateBuyOrder(buyOrder);

            if (order == null) return View(buyOrder);

            return Redirect("/orders");
        }

        [HttpPost("/trade/sell")]
        public async Task<IActionResult> SellOrder(SellOrderRequest sellOrder)
        {
            sellOrder.OrderDate = DateTime.Now;

            if (!ModelState.IsValid) return View(sellOrder);

            await _stockService.CreateSellOrder(sellOrder);

            return RedirectToAction("Orders", "Trade");
        }


        [HttpGet("/orders")]
        public async Task<IActionResult> Orders()
        {
            Orders orders = new()
            {
                BuyOrders = await _stockService.GetBuyOrders(),
                SellOrders = await _stockService.GetSellOrders(),
            };

            return View(orders);
        }
    }
}

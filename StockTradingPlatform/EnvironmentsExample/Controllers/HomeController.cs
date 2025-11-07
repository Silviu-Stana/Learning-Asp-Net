using Core.Models;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.Interfaces;

namespace Services.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<TradingOptions> _tradingOptions;
        private readonly IFinnhubService _finnhubService;

        public HomeController(IConfiguration configuration,
            IFinnhubService finnhubService, IOptions<TradingOptions> tradingOptions)
        {
            _configuration = configuration;
            _finnhubService = finnhubService;
            _tradingOptions = tradingOptions;
        }

        [Route("/{stockSymbol?}")]
        public async Task<IActionResult> Index(string? stockSymbol)
        {
            if (stockSymbol == null) stockSymbol  = _tradingOptions.Value.DefaultStockSymbol;
            decimal currentPrice=0;

            Dictionary<string,object>? stockPrice =  await _finnhubService.GetStockPriceQuote(stockSymbol);
            Dictionary<string,object>? stockProfile =  await _finnhubService.GetCompanyProfile(stockSymbol);

            StockProfile stock = new() 
            {
                StockSymbol = stockProfile["ticker"].ToString(),
                StockName = stockProfile["name"].ToString(),
                //Price = Convert.ToDouble(stockPrice["c"].ToString()),
                Price = Convert.ToDouble(stockPrice["c"].ToString()),
            };

            return View(stock);

        }
    }
}

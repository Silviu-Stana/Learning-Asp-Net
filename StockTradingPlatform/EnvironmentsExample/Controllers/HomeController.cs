using Models.Interfaces;
using Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Core.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<TradingOptions> _tradingOptions;
        private readonly IFinnhubService _finnhubService;
        private readonly IStockService _stockService;

        public HomeController(IConfiguration configuration,
            IFinnhubService finnhubService, IStockService stockService ,IOptions<TradingOptions> tradingOptions)
        {
            _configuration = configuration;
            _finnhubService = finnhubService;
            _tradingOptions = tradingOptions;
            _stockService = stockService;
        }

        [Route("/{stockSymbol?}")]
        public async Task<IActionResult> Index(string? stockSymbol)
        {
            if (stockSymbol == null) stockSymbol  = _tradingOptions.Value.DefaultStockSymbol;

            Dictionary<string,object>? stockPrice =  await _finnhubService.GetStockPriceQuote(stockSymbol);
            Dictionary<string,object>? stockProfile =  await _finnhubService.GetCompanyProfile(stockSymbol);

            StockProfile stock = new() 
            {
                StockSymbol = stockProfile!["ticker"].ToString(),
                StockName = stockProfile["name"].ToString(),
                //Price = Convert.ToDouble(stockPrice["c"].ToString()),
                Price = Convert.ToDouble(stockPrice!["c"].ToString()),
            };

            return View(stock);

        }
    }
}

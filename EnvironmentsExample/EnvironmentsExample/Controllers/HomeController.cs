using EnvironmentsExample.Models;
using EnvironmentsExample.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EnvironmentsExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IOptions<TradingOptions> _tradingOptions;
        private readonly WeatherApiOptions _options;
        private readonly IFinnhubService _finnhubService;

        public HomeController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IOptions<WeatherApiOptions> options, IFinnhubService finnhubService, IOptions<TradingOptions> tradingOptions)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _options = options.Value;
            _finnhubService = finnhubService;
            _tradingOptions = tradingOptions;
        }

        [Route("/")]
        public async Task<IActionResult> Index()
        {
            if (_tradingOptions.Value.DefaultStockSymbol == null) _tradingOptions.Value.DefaultStockSymbol = "MSFT";

            Dictionary<string,object>? stockPrice =  await _finnhubService.GetStockPriceQuote(_tradingOptions.Value.DefaultStockSymbol);

            Stock stock = new Stock() 
            {
                StockSymbol = _tradingOptions.Value.DefaultStockSymbol,
                CurrentPrice = Convert.ToDouble(stockPrice["c"].ToString()),
                LowestPrice = Convert.ToDouble(stockPrice["l"].ToString()),
                HighestPrice = Convert.ToDouble(stockPrice["h"].ToString()),
                OpenPrice = Convert.ToDouble(stockPrice["o"].ToString()),
            };

            ViewBag.CurrentEnvironment = _webHostEnvironment.EnvironmentName;
            WeatherApiOptions options = _configuration.GetSection("weatherapi").Get<WeatherApiOptions>()
                ?? throw new InvalidOperationException("Missing configuration section 'weatherapi'.");

            ViewBag.ClientID = options.ClientID;
            ViewBag.ClientSecret = _options.ClientSecret;
            return View(stock);

        }
    }
}

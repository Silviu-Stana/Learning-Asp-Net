using Models.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace Services.Services
{
    public class FinnhubService : IFinnhubService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public FinnhubService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<Dictionary<string, object>?> GetCompanyProfile(string stockSymbol)
        {
            using (HttpClient httpClient = _httpClientFactory.CreateClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://finnhub.io/api/v1/stock/profile2?symbol={stockSymbol}&token={_configuration["FinnhubToken"]}"),
                    Method = HttpMethod.Get,
                };

                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                string response = await httpResponseMessage.Content.ReadAsStringAsync();

                Dictionary<string, object>? stockProfile = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

                if (stockProfile == null) throw new InvalidOperationException("No response from Finnhub server");

                if(stockProfile.ContainsKey("error")) throw new InvalidOperationException(Convert.ToString(stockProfile["error"]));

                return stockProfile;
            }
        }

        public async Task<Dictionary<string,object>?> GetStockPriceQuote(string stockSymbol)
        {
            using (HttpClient httpClient = _httpClientFactory.CreateClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://finnhub.io/api/v1/quote?symbol={stockSymbol}&token={_configuration["FinnhubToken"]}"),
                    Method = HttpMethod.Get,
                };


                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                string response = await httpResponseMessage.Content.ReadAsStringAsync();

                Dictionary<string,object>? stockPrices = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

                if (stockPrices == null) throw new InvalidOperationException("No response from Finnhub server");

                if (stockPrices.ContainsKey("error")) throw new InvalidOperationException(Convert.ToString(stockPrices["error"]));

                return stockPrices;
            }
        }
    }
}

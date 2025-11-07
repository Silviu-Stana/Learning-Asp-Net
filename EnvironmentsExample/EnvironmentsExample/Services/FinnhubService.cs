using EnvironmentsExample.ServiceContracts;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnvironmentsExample.Services
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

                if (stockPrices == null) throw new InvalidOperationException("No response from finnhub server");

                if (stockPrices.ContainsKey("error")) throw new InvalidOperationException(Convert.ToString(stockPrices["error"]));

                return stockPrices;

                //IF I NEEDED TO STREAM A LARGE RESPONSE:
                //Stream stream = httpResponseMessage.Content.ReadAsStream();
                //StreamReader reader = new StreamReader(stream);
                // string response = reader.ReadToEnd();
            }
        }
    }
}

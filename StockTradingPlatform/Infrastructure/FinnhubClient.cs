using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Infrastructure
{
    public class FinnhubClient
    {
        private readonly string _token;
        private ClientWebSocket _ws;

        public event Action<decimal>? OnPriceUpdate;

        public FinnhubClient(string token)
        {
            _token = token;
            _ws = new ClientWebSocket();
        }

        public async Task ConnectAsync(string stockSymbol)
        {
            string url = $"wss://ws.finnhub.io?token={_token}";

            await _ws.ConnectAsync(new Uri(url), CancellationToken.None);
        }

        public async Task SubscribeAsync(string stockSymbol)
        {
            string subscribeMessage = JsonSerializer.Serialize(new { type = "subscribe", symbol = stockSymbol });
            await _ws.SendAsync(Encoding.UTF8.GetBytes(subscribeMessage), WebSocketMessageType.Text, true ,CancellationToken.None);
            _ = ReceiveMessagesAsync(); // start listening in background
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];

            while (_ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var jsonDoc = JsonDocument.Parse(message);

                    if(jsonDoc.RootElement.TryGetProperty("type", out var type) && type.GetString() == "trade")
                    {
                        var dataArray = jsonDoc.RootElement.GetProperty("data");
                        foreach (var trade in dataArray.EnumerateArray())
                        {
                            decimal price = trade.GetProperty("p").GetDecimal();
                            OnPriceUpdate?.Invoke(price);
                        }
                    }
                }
            }
        }
    }
}

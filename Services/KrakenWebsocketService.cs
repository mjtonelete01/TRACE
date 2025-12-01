using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TraceWebApi.EntityFrameworkCore;

public class KrakenWebSocketService : BackgroundService
{
    private readonly ILogger<KrakenWebSocketService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ClientWebSocket _webSocket;
    private const string KrakenWebSocketUrl = "wss://ws.kraken.com/";

    public KrakenWebSocketService(ILogger<KrakenWebSocketService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _webSocket = new ClientWebSocket();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _webSocket.ConnectAsync(new Uri(KrakenWebSocketUrl), stoppingToken);
                _logger.LogInformation("Connected to Kraken WebSocket.");

                var pairs = new HashSet<string>
                {
                    "XBT/USD", "ETH/USD", "SOL/USD", "XRP/USD", "ADA/USD", "XDG/USD",
                    "DOT/USD", "XLM/USD", "SUI/USD", "LINK/USD", "AAVE/USD", "BCH/USD",
                    "COMP/USD", "PEPE/USD", "UNI/USD", "USDC/USD", "WIF/USD", "AVAX/USD",
                    "BONK/USD", "TRUMP/USD", "XTZ/USD", "ETC/USD", "HNT/USD", "CRV/USD",
                    "ENS/USD", "ZEC/USD", "AXS/USD", "KAVA/USD", "MANA/USD", "PAXG/USD",
                    "APE/USD", "GRT/USD", "OCEAN/USD", "STORJ/USD", "1INCH/USD", "RNDR/USD",
                    "BAT/USD", "SUSHI/USD", "MINA/USD", "FTM/USD", "SNX/USD", "CHZ/USD",
                    "AUDIO/USD", "GALA/USD", "DASH/USD", "INJ/USD"
                };

                int batchSize = 20; 
                foreach (var batch in pairs.Chunk(batchSize))
                {
                    var subscribeMessage = new
                    {
                        @event = "subscribe",
                        pair = batch.ToArray(), 
                        subscription = new { name = "trade" }
                    };

                    var message = JsonSerializer.Serialize(subscribeMessage);
                    await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, stoppingToken);

                    _logger.LogInformation($"Subscribed to batch: {string.Join(", ", batch)}");

                    await Task.Delay(1000); 
                }

                await ListenForMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket error. Reconnecting in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ListenForMessages(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var buffer = new byte[8192]; // ✅ Increased buffer size
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("Kraken WebSocket closed.");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", stoppingToken);
                    break;
                }

                var jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessTradeData(jsonString);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket messages.");
        }
    }


    private async Task ProcessTradeData(string jsonData)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 4)
                return;

            var tradeArray = root[1]; 
            var pair = root[3].GetString();

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TraceAppDbContext>();

            // ✅ List of special pairs with a lower threshold
            var specialPairs = new HashSet<string>
            {
                "WIF/USD", "BONK/USD", "XTZ/USD", "COMP/USD",
                "HNT/USD", "CRV/USD", "ENS/USD", "ZEC/USD",
                "AXS/USD", "KAVA/USD", "MANA/USD", "PAXG/USD",
                "APE/USD", "GRT/USD", "OCEAN/USD", "STORJ/USD",
                "1INCH/USD", "RNDR/USD", "BAT/USD", "SUSHI/USD",
                "MINA/USD", "FTM/USD", "SNX/USD", "CHZ/USD",
                "AUDIO/USD", "GALA/USD", "DASH/USD", "INJ/USD"
            };


            foreach (var trade in tradeArray.EnumerateArray())
            {
                decimal price = decimal.Parse(trade[0].GetString() ?? string.Empty, System.Globalization.CultureInfo.InvariantCulture);
                decimal volume = decimal.Parse(trade[1].GetString() ?? string.Empty, System.Globalization.CultureInfo.InvariantCulture);
                decimal totalValue = price * volume; 

                bool isAccepted = totalValue > 12000 || (specialPairs.Contains(pair ?? string.Empty) && totalValue > 5000);

                if (isAccepted)
                {
                    var tradeModel = new CryptoTrade
                    {
                        Pair = pair ?? string.Empty,
                        Price = price,
                        Volume = volume,
                        Time = DateTimeOffset.FromUnixTimeSeconds((long)double.Parse(trade[2].GetString() ?? string.Empty, System.Globalization.CultureInfo.InvariantCulture)).UtcDateTime,
                        Type = trade[3].GetString() == "b" ? "b" : "s",
                        Total = totalValue
                    };

                    Console.WriteLine("Added: " + tradeModel.Pair + " Total: " + totalValue);

                    dbContext.CryptoTrades.Add(tradeModel);
                }
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trade data.");
        }
    }

}

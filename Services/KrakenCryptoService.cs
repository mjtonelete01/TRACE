using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TraceWebApi.EntityFrameworkCore;
using TraceWebApi.Helpers;
using TraceWebApi.Interfaces;
using TraceWebApi.Responses.Cryptos;

namespace TraceWebApi.Services;

public class KrakenCryptoService : ICryptoService
{

    private readonly string KRAKEN_REST_URL = "https://api.kraken.com/0/public/Trades";
    private readonly HttpClient _httpClient;
    private readonly TraceAppDbContext _dbContext;
    private readonly ILogger<KrakenCryptoService> _logger;

    public KrakenCryptoService(HttpClient httpClient, TraceAppDbContext dbContext, ILogger<KrakenCryptoService> logger)
    {
        _httpClient = httpClient;
         _dbContext = dbContext;
         _logger = logger;
    }

    public async Task<decimal> GetRecentTradesAISentimentAsync(string pair)
    {
        var count = 1000; 
        var totalBuy = 0m;
        var totalSell = 0m;

        var utcNow = DateTime.UtcNow;
        var oneDayAgo = utcNow.AddHours(-24); // 24 hours ago
        var oneDayAgoTimestamp = DateTimeHelper.DateTimeToTimeStamp(oneDayAgo);
        var sinceTimestamp = oneDayAgoTimestamp.ToString(); // Start from 24 hours ago

        try
        {
            using var client = _httpClient;

            do
            {
                var url = $"{KRAKEN_REST_URL}?pair={pair}&count={count}&since={sinceTimestamp}";

                var response = await client.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<KrakenApiResponse>(response);

                if (result?.Error != null && result.Error.Any())
                {
                    Console.WriteLine($"Error fetching trades: {string.Join(", ", result.Error)}");
                    throw new Exception($"API error: {string.Join(", ", result.Error)}");
                }

                var resultAsList = result!.Result!.Cast<object>().ToList();
                dynamic pairTrades = resultAsList[0];

                dynamic trades = pairTrades.Value!;

                if (trades == null || trades!.Count == 0)
                    break;

                foreach (var trade in trades!)
                {
                    var price = Convert.ToDecimal(trade[0]);
                    var volume = Convert.ToDecimal(trade[1]);
                    var side = Convert.ToString(trade[3]);

                    if (price < 12000)
                    {
                        continue;
                    }


                    if (side == "b")
                    {
                        totalBuy += price * volume;
                    }
                    else
                    {
                        totalSell += price * volume;
                    }
                }

                if (trades.Count < count)
                    break;

                var lastTradeTimestamp = Convert.ToDouble(trades[trades.Count - 1][2]);

                sinceTimestamp = lastTradeTimestamp.ToString();

            } while (true);  // Continue until there are no more trades

            return totalBuy / (totalBuy + totalSell) * 100;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing trades: {ex.Message}");
            throw;
        }
    }

    public async Task<List<RecentTransactionResponse>> GetTradesPaginationAsync(
        string pair,
        int pageSize = 10,
        double? lastTradeUnixTimeStamp = null)
    {
        var utcNow = DateTime.UtcNow;
        var oneDayAgo = utcNow.AddHours(-24); // 24 hours ago
        var oneDayAgoTimestamp = DateTimeHelper.DateTimeToTimeStamp(oneDayAgo);

        var count = lastTradeUnixTimeStamp.HasValue ? pageSize + 1 : pageSize;

        string url = string.Concat(KRAKEN_REST_URL, $"?pair={pair}&count={count}", lastTradeUnixTimeStamp.HasValue ? $"&since={lastTradeUnixTimeStamp}" : $"&since={oneDayAgoTimestamp}");


        var response = await _httpClient.GetStringAsync(url);
        var result = JsonConvert.DeserializeObject<KrakenApiResponse>(response);

        var currency = pair.Substring(3);
        var coin = pair.Substring(0, 3);
        var resultAsList = result!.Result!.Cast<object>().ToList();
        dynamic pairTrades = resultAsList[0];

        List<List<object>> trades = JsonConvert.DeserializeObject<List<List<object>>>(pairTrades.Value.ToString());
        
        return trades!.Select(trade => new RecentTransactionResponse
        {
            TransactionId = Convert.ToInt64(trade[6])!,
            UnixDate = Convert.ToDouble(trade[2]),
            Currency = currency,
            Price = Convert.ToDecimal(trade[0]),
            Volume = Convert.ToDecimal(trade[1]),
            Coin = coin,
            Status = Convert.ToString(trade[3]) == "b" ? "Buy" : "Sell",
            Color = Convert.ToString(trade[3]) == "b" ? "success" : "warning"
        })
        .Where(trade => trade.UnixDate != lastTradeUnixTimeStamp)
        .ToList() ?? new List<RecentTransactionResponse>();

    }

    public async Task<(decimal sentimentPercentage, string sentiment)> CalculateSentimentAsync(string pair, string timeframe)
    {
        if (string.IsNullOrEmpty(pair) || string.IsNullOrEmpty(timeframe))
            throw new ArgumentException("Pair and timeframe are required.");

        DateTime cutoffTime = GetTimeframeCutoff(timeframe);

        var trades = await _dbContext.CryptoTrades
            .Where(t => t.Pair == pair && t.Time >= cutoffTime)
            .ToListAsync();

        if (!trades.Any())
            return (50, "Neutral"); 

        decimal totalBuy = trades.Where(t => t.Type == "b").Sum(t => t.Volume);
        decimal totalSell = trades.Where(t => t.Type == "s").Sum(t => t.Volume);
        decimal totalTransactions = totalBuy + totalSell;

        if (totalTransactions == 0)
            return (50, "Neutral"); 

        decimal sentimentPercentage = (totalBuy / totalTransactions) * 100;
        string sentiment = sentimentPercentage > 50 ? "Bullish" : "Bearish";

        return (Math.Round(sentimentPercentage, 2), sentiment);
    }

    public async Task<List<CryptoTrade>> GetRecentTradesAsync(string pair, string timeframe)
    {
        if (string.IsNullOrEmpty(pair) || string.IsNullOrEmpty(timeframe))
            throw new ArgumentException("Pair and timeframe are required.");

        DateTime cutoffTime = GetTimeframeCutoff(timeframe);

        return await _dbContext.CryptoTrades
            .Where(t => t.Pair == pair && t.Time >= cutoffTime)
            .OrderByDescending(t => t.Time).Take(10) 
            .ToListAsync();
    }

    public async Task<CryptoTrade?> GetLargestTradeAsync(string pair, string timeframe)
    {
        if (string.IsNullOrEmpty(pair) || string.IsNullOrEmpty(timeframe))
            throw new ArgumentException("Pair and timeframe are required.");

        DateTime cutoffTime = GetTimeframeCutoff(timeframe);

        return await _dbContext.CryptoTrades
            .Where(t => t.Pair == pair && t.Time >= cutoffTime)
            .OrderByDescending(t => t.Total) 
            .FirstOrDefaultAsync(); 
    }


    private DateTime GetTimeframeCutoff(string timeframe)
    {
        return timeframe.ToLower() switch
        {
            "15m" => DateTime.UtcNow.AddMinutes(-15),
            "30m" => DateTime.UtcNow.AddMinutes(-30),
            "1h" => DateTime.UtcNow.AddHours(-1),
            "6h" => DateTime.UtcNow.AddHours(-6),
            "12h" => DateTime.UtcNow.AddHours(-12),
            "1d" => DateTime.UtcNow.AddDays(-1),
            "1w" => DateTime.UtcNow.AddDays(-7),
            _ => DateTime.UtcNow.AddHours(-1) // Default to 1 hour if invalid
        };
    }

    //public async Task<List<RecentTransactionResponse>> GetRecentTradesAsync(List<string> pairs)
    //{

    //    var tasks = pairs.Select(pair => GetRecentTradesAsync(pair)).ToList();
    //    var results = await Task.WhenAll(tasks);

    //    return results
    //        .SelectMany(result => result)
    //        .OrderByDescending(trade => trade.Total)
    //        .ToList();
    //}
}

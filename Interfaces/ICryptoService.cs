using TraceWebApi.Responses.Cryptos;

namespace TraceWebApi.Interfaces;

public interface ICryptoService
{
    Task<decimal> GetRecentTradesAISentimentAsync(string pair);
    Task<List<RecentTransactionResponse>> GetTradesPaginationAsync(
        string pair,
        int pageSize = 10,
        double? lastTradeUnixTimeStamp = null);
    Task<(decimal sentimentPercentage, string sentiment)> CalculateSentimentAsync(string pair, string timeframe);
    Task<List<CryptoTrade>> GetRecentTradesAsync(string pair, string timeframe);
    Task<CryptoTrade?> GetLargestTradeAsync(string pair, string timeframe);
    //Task<List<RecentTransactionResponse>> GetRecentTradesAsync(List<string> pairs);
}

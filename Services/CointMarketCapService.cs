using System.Text.Json;
using Newtonsoft.Json;
using TraceWebApi.Helpers;
using TraceWebApi.Interfaces;
using TraceWebApi.Models.Cryptos;
using TraceWebApi.Responses.Cryptos;

namespace TraceWebApi.Services;

public class CoinMarketCapService
{
    private async Task<List<CryptoData>> GetMarketDataFromCMC()
    {
       using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", "YOUR_API_KEY");

        string url = "https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest";
        
        var response = await httpClient.GetStringAsync(url);
        
        var result = System.Text.Json.JsonSerializer.Deserialize<CoinMarketCapResponse>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Ensures it can map JSON properties even if casing is different
        });

        return null;
    }

}
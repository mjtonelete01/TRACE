using TraceWebApi.Models.Cryptos;
using TraceWebApi.Responses.Cryptos;

namespace TraceWebApi.Interfaces;

public interface ICoinMarketCapService
{
    Task<List<CryptoData>> GetMarketDataFromCMC();
}

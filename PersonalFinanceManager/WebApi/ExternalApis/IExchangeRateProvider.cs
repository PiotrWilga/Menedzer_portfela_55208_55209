using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.ExternalApis;

public interface IExchangeRateProvider
{
    Task<ExchangeRateDto?> GetCurrentRateAsync(string baseCurrency, string targetCurrency);
}

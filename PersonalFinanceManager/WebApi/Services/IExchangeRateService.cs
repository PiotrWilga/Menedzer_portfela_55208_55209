using PersonalFinanceManager.WebApi.Dtos;

namespace WebApi.Services;

public interface IExchangeRateService
{
    Task<CurrentExchangeConversionDto?> ConvertCurrentAsync(string baseCurrency, string targetCurrency, decimal amount);
}

using PersonalFinanceManager.WebApi.Dtos;

public interface IHistoricalExchangeRateService
{
    Task<HistoricalExchangeConversionDto?> ConvertAsync(DateTime date, string baseCurrency, string targetCurrency, decimal amount);
}

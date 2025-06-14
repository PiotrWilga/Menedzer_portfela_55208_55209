using PersonalFinanceManager.WebApi.Dtos;
using System.Text.Json;

public class NbpHistoricalExchangeRateService : IHistoricalExchangeRateService
{
    private readonly HttpClient _httpClient;

    public NbpHistoricalExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HistoricalExchangeConversionDto?> ConvertAsync(DateTime date, string baseCurrency, string targetCurrency, decimal amount)
    {
        if (baseCurrency == targetCurrency)
        {
            return new HistoricalExchangeConversionDto
            {
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
                SourceAmount = amount,
                ConvertedAmount = amount,
                RateDate = date
            };
        }

        async Task<decimal?> GetRateAsync(string currency)
        {
            if (currency.ToUpper() == "PLN")
                return 1m;

            var url = $"https://api.nbp.pl/api/exchangerates/rates/A/{currency}/{date:yyyy-MM-dd}/?format=json";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("rates", out var ratesArray) || ratesArray.GetArrayLength() == 0)
                return null;

            return ratesArray[0].GetProperty("mid").GetDecimal();
        }

        var baseRate = await GetRateAsync(baseCurrency);
        var targetRate = await GetRateAsync(targetCurrency);

        if (baseRate is null || targetRate is null)
            return null;

        var plnAmount = amount * baseRate.Value;
        var converted = plnAmount / targetRate.Value;

        return new HistoricalExchangeConversionDto
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            SourceAmount = amount,
            ConvertedAmount = Math.Round(converted, 4),
            RateDate = date
        };
    }
}

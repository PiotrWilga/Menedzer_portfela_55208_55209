using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.WebApi.Dtos;

namespace WebApi.Services
{
    public class ExchangeRateApiService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;
        private readonly TimeSpan _cacheDuration;

        public ExchangeRateApiService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;

            var section = configuration.GetSection("ExchangeRateApi");
            _apiKey = section["Key"];
            var minutes = int.TryParse(section["CacheDurationMinutes"], out var m) ? m : 5;
            _cacheDuration = TimeSpan.FromMinutes(minutes);
        }

        public async Task<CurrentExchangeConversionDto?> GetCurrentRateAsync(string baseCurrency, string targetCurrency)
        {
            var cacheKey = $"exchange:{baseCurrency}->{targetCurrency}";
            if (_cache.TryGetValue(cacheKey, out CurrentExchangeConversionDto cached))
                return cached;

            var url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/latest/{baseCurrency}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.GetProperty("result").GetString() != "success")
                return null;

            if (!root.TryGetProperty("conversion_rates", out var rates))
                return null;

            if (!rates.TryGetProperty(targetCurrency, out var rateElement))
                return null;

            var dto = new CurrentExchangeConversionDto
            {
                Source = "ExchangeRate-API",
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
                Rate = rateElement.GetDecimal(),
                Timestamp = DateTime.UtcNow
            };

            _cache.Set(cacheKey, dto, _cacheDuration);

            return dto;
        }
        public async Task<CurrentExchangeConversionDto?> ConvertCurrentAsync(string baseCurrency, string targetCurrency, decimal amount)
        {
            var rateDto = await GetCurrentRateAsync(baseCurrency, targetCurrency);
            if (rateDto is null)
                return null;

            var convertedAmount = Math.Round(amount * rateDto.Rate, 4);

            return new CurrentExchangeConversionDto
            {
                Source = rateDto.Source,
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
                Rate = rateDto.Rate,
                SourceAmount = amount,
                ConvertedAmount = convertedAmount,
                Timestamp = rateDto.Timestamp
            };
        }

    }
}

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.ExternalApis
{
    public class ExchangeRateApiProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;
        private readonly TimeSpan _cacheDuration;

        public ExchangeRateApiProvider(
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

        public async Task<ExchangeRateDto?> GetCurrentRateAsync(string baseCurrency, string targetCurrency)
        {
            var cacheKey = $"exchange:{baseCurrency}->{targetCurrency}";
            if (_cache.TryGetValue(cacheKey, out ExchangeRateDto cached))
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

            var dto = new ExchangeRateDto
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
    }
}

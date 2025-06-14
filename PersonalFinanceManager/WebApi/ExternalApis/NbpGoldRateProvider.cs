using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace PersonalFinanceManager.WebApi.ExternalApis
{
    public class NbpGoldRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public NbpGoldRateProvider(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<decimal?> GetGoldPricePerGramInPLNAsync()
        {
            const string cacheKey = "NBP_GOLD_PRICE";
            if (_cache.TryGetValue(cacheKey, out decimal cached))
                return cached;

            var url = "https://api.nbp.pl/api/cenyzlota?format=json";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                return null;

            var goldPrice = doc.RootElement[0].GetProperty("cena").GetDecimal();

            _cache.Set(cacheKey, goldPrice, TimeSpan.FromHours(1));
            return goldPrice;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PersonalFinanceManager.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public ExchangeRateController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;

            var rawValue = config["ExchangeRateApi:AccessKey"];
            _apiKey = rawValue?.StartsWith("env:") == true
                ? Environment.GetEnvironmentVariable(rawValue.Substring(4))
                : rawValue;
        }

        [HttpGet("{baseCurrency}")]
        public async Task<IActionResult> GetRates(string baseCurrency)
        {
            var url = $"https://v6.exchangerate-api.com/v6/klucz-api/latest/{baseCurrency.ToUpper()}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Błąd przy pobieraniu danych z zewnętrznego API.");
            }

            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
    }
}

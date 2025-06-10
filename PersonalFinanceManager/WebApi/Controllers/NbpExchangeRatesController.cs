using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/nbp")]
public class NbpExchangeRatesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NbpExchangeRatesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: api/nbp/exchange-rate/usd
    [HttpGet("exchange-rate/{currencyCode}")]
    public async Task<IActionResult> GetExchangeRate(string currencyCode)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var url = $"https://api.nbp.pl/api/exchangerates/rates/a/{currencyCode}?format=json";

        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Błąd podczas pobierania danych z NBP");

            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonDocument.Parse(json);

            return Ok(parsed.RootElement);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Błąd połączenia z API NBP: {ex.Message}");
        }
    }
}

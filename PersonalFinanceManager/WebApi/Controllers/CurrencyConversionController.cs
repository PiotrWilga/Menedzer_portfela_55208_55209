using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CurrencyConversionController : ControllerBase
{
    private readonly IHistoricalExchangeRateService _service;

    public CurrencyConversionController(IHistoricalExchangeRateService service)
    {
        _service = service;
    }

    [HttpGet("historical-conversion")]
    public async Task<IActionResult> Convert(
        [FromQuery] DateTime date,
        [FromQuery] string baseCurrency,
        [FromQuery] string targetCurrency,
        [FromQuery] decimal amount)
    {
        var result = await _service.ConvertAsync(date, baseCurrency, targetCurrency, amount);
        if (result == null)
            return NotFound("Brak danych dla podanej daty lub waluty.");

        return Ok(result);
    }
}

using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.ExternalApis;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/rates")]
public class RatesController : ControllerBase
{
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public RatesController(IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRateProvider = exchangeRateProvider;
    }

    [HttpGet("current")]
    public async Task<ActionResult<ExchangeRateDto>> GetCurrentRate([FromQuery] string baseCurrency, [FromQuery] string targetCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
            return BadRequest("Base and target currencies must be provided.");

        var result = await _exchangeRateProvider.GetCurrentRateAsync(baseCurrency.ToUpper(), targetCurrency.ToUpper());

        return result == null ? NotFound("Rate not found.") : Ok(result);
    }
}

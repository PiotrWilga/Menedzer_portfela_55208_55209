using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Dtos;
using WebApi.Services;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/rates")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateProvider;
    private readonly IHistoricalExchangeRateService _historicalService;

    public ExchangeRatesController(
        IExchangeRateService exchangeRateProvider,
        IHistoricalExchangeRateService historicalService)
    {
        _exchangeRateProvider = exchangeRateProvider;
        _historicalService = historicalService;
    }

    [HttpGet("Historical")]
    [ProducesResponseType(typeof(HistoricalExchangeConversionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConvertHistoricalRate(
    [FromQuery] DateTime date,
    [FromQuery] string baseCurrency,
    [FromQuery] string targetCurrency,
    [FromQuery] decimal amount)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
            return BadRequest("Base and target currencies must be provided.");

        if (amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var result = await _historicalService.ConvertAsync(date, baseCurrency.ToUpper(), targetCurrency.ToUpper(), amount);

        if (result is null)
            return NotFound("Rate data not available for the given date and currencies.");

        // Obliczenie kursu
        var rate = result.SourceAmount != 0
            ? Math.Round(result.ConvertedAmount / result.SourceAmount, 4)
            : 0;

        // Dodanie dodatkowych informacji
        var enriched = new HistoricalExchangeConversionDto
        {
            BaseCurrency = result.BaseCurrency,
            TargetCurrency = result.TargetCurrency,
            SourceAmount = result.SourceAmount,
            ConvertedAmount = result.ConvertedAmount,
            RateDate = result.RateDate,
            Source = "NBP",
            Rate = rate
        };

        return Ok(enriched);
    }


    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRateAndConvert(
    [FromQuery] string baseCurrency,
    [FromQuery] string targetCurrency,
    [FromQuery] decimal amount)
    {
        var result = await _exchangeRateProvider.ConvertCurrentAsync(baseCurrency, targetCurrency, amount);
        if (result is null)
            return NotFound();

        return Ok(result);
    }
}

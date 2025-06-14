namespace PersonalFinanceManager.WebApi.Dtos;

public class ExchangeRateDto
{
    public string Source { get; set; } = default!;
    public string BaseCurrency { get; set; } = default!;
    public string TargetCurrency { get; set; } = default!;
    public decimal Rate { get; set; }
    public DateTime Timestamp { get; set; }
}

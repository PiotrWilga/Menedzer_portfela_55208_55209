namespace PersonalFinanceManager.WebApi.Dtos;
public class HistoricalExchangeConversionDto
{
    public DateTime RateDate { get; set; }
    public decimal SourceAmount { get; set; }
    public string BaseCurrency { get; set; } = default!;
    public decimal? Rate { get; set; }
    public decimal ConvertedAmount { get; set; }
    public string TargetCurrency { get; set; } = default!;
    public string? Source { get; set; }
}

namespace PersonalFinanceManager.WebApi.Dtos;
public class CurrentExchangeConversionDto
{
    public DateTime Timestamp { get; set; }
    public decimal SourceAmount { get; set; }
    public string BaseCurrency { get; set; } = null!;
    public decimal Rate { get; set; }
    public decimal ConvertedAmount { get; set; }
    public string TargetCurrency { get; set; } = null!;
    public string Source { get; set; } = null!;
}

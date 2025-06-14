namespace PersonalFinanceManager.WebApi.Dtos;

public class HistoricalExchangeConversionDto
{
    public string Source { get; set; } = "NBP - Tabela A";
    public string BaseCurrency { get; set; } = default!;
    public string TargetCurrency { get; set; } = default!;
    public decimal SourceAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public DateTime RateDate { get; set; }
}

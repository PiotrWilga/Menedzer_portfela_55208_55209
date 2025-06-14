namespace PersonalFinanceManager.WebApi.Dtos;

public class GoldConversionDto
{
    public string Source { get; set; } = "NBP";
    public string Currency { get; set; } = default!;
    public decimal GoldPriceInPLN { get; set; }
    public decimal CurrencyRateToPLN { get; set; }
    public decimal GramsOfGold { get; set; }
    public DateTime Timestamp { get; set; }
}

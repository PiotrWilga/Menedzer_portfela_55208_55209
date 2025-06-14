// Dto/CreateTransactionDto.cs
using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.WebApi.Dtos;

public class CreateTransactionDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; } // Kwota w walucie konta

    public DateTime? Date { get; set; }

    public int? CategoryId { get; set; }

    // Dane dla transakcji w innej walucie
    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrencyCode { get; set; } // np. "USD"
    public decimal? ExchangeRate { get; set; } // np. 4.05 (PLN/USD)
}
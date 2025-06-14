// Dto/UpdateTransactionDto.cs
using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.WebApi.Dtos;

public class UpdateTransactionDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal? Amount { get; set; } // Kwota w walucie konta

    public DateTime? Date { get; set; }

    public int? CategoryId { get; set; }

    // Dane dla transakcji w innej walucie (opcjonalne do aktualizacji)
    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
}
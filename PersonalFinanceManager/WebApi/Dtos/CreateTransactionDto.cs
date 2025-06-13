using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Dtos;

public class CreateTransactionDto
{
    [Required]
    [MinLength(2, ErrorMessage = "Transaction name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Transaction name cannot contain only whitespace.")]
    public string Name { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public DateTime? Date { get; set; }

    [Required]
    public int AccountId { get; set; }

    public int? CategoryId { get; set; }

    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
}
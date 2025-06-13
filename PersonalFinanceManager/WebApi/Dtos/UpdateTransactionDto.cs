using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.WebApi.Dtos;

public class UpdateTransactionDto
{
    [MinLength(2, ErrorMessage = "Transaction name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Transaction name cannot contain only whitespace.")]
    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public int? CategoryId { get; set; }

    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }
}
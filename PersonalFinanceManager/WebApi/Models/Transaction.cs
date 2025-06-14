using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceManager.WebApi.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Transaction name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Transaction name cannot contain only whitespace.")]
    public string Name { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account Account { get; set; }

    public int? CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }

    public int OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public AppUser Owner { get; set; }
}
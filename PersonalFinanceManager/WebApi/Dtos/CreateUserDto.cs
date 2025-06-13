using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Dtos;
public class CreateAccountDto
{
    [Required]
    [MinLength(2, ErrorMessage = "Account name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Account name cannot contain only whitespace.")]
    public string Name { get; set; }

    [Required]
    public AccountType Type { get; set; }

    [Required]
    public string CurrencyCode { get; set; }

    public decimal Balance { get; set; }

    public bool ShowInSummary { get; set; }
}


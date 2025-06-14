using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Dtos;

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public AccountType Type { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Balance { get; set; }
    public bool ShowInSummary { get; set; }
    public int OwnerId { get; set; }
    public string OwnerLogin { get; set; } // Dodana właściwość dla loginu właściciela
}
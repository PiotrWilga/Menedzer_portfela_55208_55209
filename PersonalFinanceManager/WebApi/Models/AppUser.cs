using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.WebApi.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string DefaultCurrency { get; set; } = "PLN";

    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Account> OwnedAccounts { get; set; }
    public ICollection<AccountPermission> AccountPermissions { get; set; }
    public ICollection<Category> OwnedCategories { get; set; }
    public ICollection<Transaction> OwnedTransactions { get; set; }
}

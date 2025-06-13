using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceManager.WebApi.Models;

public class Account
{
    [Key]
    [Column("AccountId")]
    public int Id { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Account name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Account name cannot contain only whitespace.")]
    public string Name { get; set; }

    public AccountType Type { get; set; }

    [Required]
    public string CurrencyCode { get; set; }

    public decimal Balance { get; set; }

    public bool ShowInSummary { get; set; }

    public int OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public AppUser Owner { get; set; }

    public ICollection<AccountPermission> AccountPermissions { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}
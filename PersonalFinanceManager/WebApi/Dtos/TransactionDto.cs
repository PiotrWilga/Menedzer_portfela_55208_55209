using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Dtos;

public class TransactionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public int AccountId { get; set; }
    public string AccountName { get; set; }
    public string AccountCurrencyCode { get; set; }
    public AccountType AccountType { get; set; }

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColor { get; set; }

    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrencyCode { get; set; }
    public decimal? ExchangeRate { get; set; }

    public int OwnerId { get; set; }
    public string OwnerLogin { get; set; }
}
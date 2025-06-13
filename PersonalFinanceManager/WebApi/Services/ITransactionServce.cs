using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Services;

public interface ITransactionService
{
    IEnumerable<Transaction> GetAll(int userId, int? accountId = null);
    Transaction GetById(int id);
    // Zwraca Transaction i opcjonalnie poprzednią wartość do skorygowania balansu
    (Transaction? Transaction, decimal OldAmount, int OldAccountId) Create(CreateTransactionDto transactionDto, int ownerId, out string? errorMessage);
    (Transaction? Transaction, decimal OldAmount, int OldAccountId) Update(int id, UpdateTransactionDto transactionDto, int userId, out string? errorMessage);
    bool Delete(int id, int userId, out decimal oldAmount, out int oldAccountId);
}
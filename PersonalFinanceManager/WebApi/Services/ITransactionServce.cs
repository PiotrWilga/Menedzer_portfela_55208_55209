// Services/ITransactionService.cs
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Services;

public interface ITransactionService
{
    IEnumerable<TransactionDto> GetAll(int accountId, int userId);
    TransactionDto GetById(int transactionId, int accountId);
    (Transaction? Transaction, decimal OldAmount, int OldAccountId) Create(int accountId, CreateTransactionDto transactionDto, int ownerId, out string? errorMessage);
    (Transaction? Transaction, decimal OldAmount, int OldAccountId) Update(int transactionId, int accountId, UpdateTransactionDto transactionDto, int userId, out string? errorMessage);
    bool Delete(int transactionId, int accountId, int userId, out decimal oldAmount, out int oldAccountId);
}
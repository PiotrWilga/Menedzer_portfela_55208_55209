using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Dtos;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceManager.WebApi.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;
    private readonly IAccountService _accountService; 

    public TransactionService(AppDbContext context, IAccountService accountService)
    {
        _context = context;
        _accountService = accountService;
    }

    public IEnumerable<Transaction> GetAll(int userId, int? accountId = null)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Owner)
            .Where(t => t.OwnerId == userId); 

        if (accountId.HasValue)
        {
            query = query.Where(t => t.AccountId == accountId.Value);
        }

        return query.ToList();
    }

    public Transaction GetById(int id)
    {
        return _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.Owner)
            .FirstOrDefault(t => t.Id == id);
    }

    public (Transaction? Transaction, decimal OldAmount, int OldAccountId) Create(CreateTransactionDto transactionDto, int ownerId, out string? errorMessage)
    {
        errorMessage = null;
        decimal oldAmount = 0; 
        int oldAccountId = 0; 

        
        if (!_accountService.HasAccountAccess(transactionDto.AccountId, ownerId, requireWriteAccess: true))
        {
            errorMessage = "Account not found or you do not have write access to it.";
            return (null, oldAmount, oldAccountId);
        }

        var accountCurrency = _accountService.GetAccountCurrency(transactionDto.AccountId);
        if (accountCurrency == null)
        {
            errorMessage = "Account currency could not be determined.";
            return (null, oldAmount, oldAccountId);
        }

        var transaction = new Transaction
        {
            Name = transactionDto.Name,
            Address = transactionDto.Address,
            Description = transactionDto.Description,
            Date = transactionDto.Date ?? DateTime.UtcNow, 
            AccountId = transactionDto.AccountId,
            CategoryId = transactionDto.CategoryId,
            OwnerId = ownerId 
        };

        
        if (!string.IsNullOrEmpty(transactionDto.OriginalCurrencyCode) && transactionDto.OriginalCurrencyCode != accountCurrency)
        {
            if (!transactionDto.OriginalAmount.HasValue || !transactionDto.ExchangeRate.HasValue || transactionDto.ExchangeRate.Value <= 0)
            {
                errorMessage = "Original amount and exchange rate are required for cross-currency transactions.";
                return (null, oldAmount, oldAccountId);
            }
            transaction.OriginalAmount = transactionDto.OriginalAmount.Value;
            transaction.OriginalCurrencyCode = transactionDto.OriginalCurrencyCode;
            transaction.ExchangeRate = transactionDto.ExchangeRate.Value;
            
            
            transaction.Amount = transactionDto.OriginalAmount.Value * transactionDto.ExchangeRate.Value;
        }
        else
        {
            transaction.Amount = transactionDto.Amount;
        }

        _context.Transactions.Add(transaction);
        _context.SaveChanges();

        if (!_accountService.UpdateAccountBalance(transaction.AccountId, transaction.Amount))
        {
            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
            errorMessage = "Failed to update account balance.";
            return (null, oldAmount, oldAccountId);
        }

        return (transaction, oldAmount, oldAccountId);
    }

    public (Transaction? Transaction, decimal OldAmount, int OldAccountId) Update(int id, UpdateTransactionDto transactionDto, int userId, out string? errorMessage)
    {
        errorMessage = null;
        var existingTransaction = _context.Transactions.Find(id);
        if (existingTransaction == null)
        {
            errorMessage = "Transaction not found.";
            return (null, 0, 0);
        }

        if (existingTransaction.OwnerId != userId)
        {
            errorMessage = "You do not have permission to update this transaction.";
            return (null, 0, 0);
        }

        if (!_accountService.HasAccountAccess(existingTransaction.AccountId, userId, requireWriteAccess: true))
        {
            errorMessage = "You do not have write access to the associated account.";
            return (null, 0, 0);
        }

        decimal oldAmount = existingTransaction.Amount;
        int oldAccountId = existingTransaction.AccountId;

        existingTransaction.Name = transactionDto.Name ?? existingTransaction.Name;
        existingTransaction.Address = transactionDto.Address ?? existingTransaction.Address;
        existingTransaction.Description = transactionDto.Description ?? existingTransaction.Description;
        existingTransaction.Date = transactionDto.Date ?? existingTransaction.Date;
        existingTransaction.CategoryId = transactionDto.CategoryId ?? existingTransaction.CategoryId;

        if (transactionDto.OriginalCurrencyCode != null && transactionDto.OriginalCurrencyCode != _accountService.GetAccountCurrency(existingTransaction.AccountId))
        {
            if (!transactionDto.OriginalAmount.HasValue || !transactionDto.ExchangeRate.HasValue || transactionDto.ExchangeRate.Value <= 0)
            {
                errorMessage = "Original amount and exchange rate are required for cross-currency transactions.";
                return (null, oldAmount, oldAccountId);
            }
            existingTransaction.OriginalAmount = transactionDto.OriginalAmount.Value;
            existingTransaction.OriginalCurrencyCode = transactionDto.OriginalCurrencyCode;
            existingTransaction.ExchangeRate = transactionDto.ExchangeRate.Value;
            existingTransaction.Amount = transactionDto.OriginalAmount.Value * transactionDto.ExchangeRate.Value;
        }
        else if (transactionDto.Amount.HasValue)
        {
            existingTransaction.Amount = transactionDto.Amount.Value;
            existingTransaction.OriginalAmount = null;
            existingTransaction.OriginalCurrencyCode = null;
            existingTransaction.ExchangeRate = null;
        }


        _context.Transactions.Update(existingTransaction);
        _context.SaveChanges();

        _accountService.UpdateAccountBalance(oldAccountId, -oldAmount);
        _accountService.UpdateAccountBalance(existingTransaction.AccountId, existingTransaction.Amount);

        return (existingTransaction, oldAmount, oldAccountId);
    }

    public bool Delete(int id, int userId, out decimal oldAmount, out int oldAccountId)
    {
        oldAmount = 0;
        oldAccountId = 0;

        var transaction = _context.Transactions.Find(id);
        if (transaction == null) return false;

        if (transaction.OwnerId != userId)
        {
            return false;
        }

                if (!_accountService.HasAccountAccess(transaction.AccountId, userId, requireWriteAccess: true))
        {
            return false;
        }

        oldAmount = transaction.Amount;
        oldAccountId = transaction.AccountId;

        _context.Transactions.Remove(transaction);
        _context.SaveChanges();

        _accountService.UpdateAccountBalance(oldAccountId, -oldAmount);

        return true;
    }
}
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

    public IEnumerable<TransactionDto> GetAll(int accountId, int userId)
    {
        if (!_accountService.HasAccountAccess(accountId, userId))
        {
            return Enumerable.Empty<TransactionDto>();
        }

        return _context.Transactions
                       .Include(t => t.Account)
                           .ThenInclude(a => a.Owner)
                       .Include(t => t.Category)
                       .Include(t => t.Owner)
                       .Where(t => t.AccountId == accountId && t.OwnerId == userId)
                       .Select(t => new TransactionDto
                       {
                           Id = t.Id,
                           Name = t.Name,
                           Address = t.Address,
                           Description = t.Description,
                           Amount = t.Amount,
                           Date = t.Date,
                           AccountId = t.AccountId,
                           AccountName = t.Account.Name,
                           AccountCurrencyCode = t.Account.CurrencyCode,
                           AccountType = t.Account.Type,
                           CategoryId = t.CategoryId,
                           CategoryName = t.Category != null ? t.Category.Name : null,
                           CategoryColor = t.Category != null ? t.Category.Color : null,
                           OriginalAmount = t.OriginalAmount,
                           OriginalCurrencyCode = t.OriginalCurrencyCode,
                           ExchangeRate = t.ExchangeRate,
                           OwnerId = t.OwnerId,
                           OwnerLogin = t.Owner.Login
                       })
                       .ToList();
    }

    public TransactionDto GetById(int transactionId, int accountId)
    {
        var transaction = _context.Transactions
            .Include(t => t.Account)
                .ThenInclude(a => a.Owner)
            .Include(t => t.Category)
            .Include(t => t.Owner)
            .FirstOrDefault(t => t.Id == transactionId && t.AccountId == accountId);

        if (transaction == null) return null;

        return new TransactionDto
        {
            Id = transaction.Id,
            Name = transaction.Name,
            Address = transaction.Address,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date,
            AccountId = transaction.AccountId,
            AccountName = transaction.Account.Name,
            AccountCurrencyCode = transaction.Account.CurrencyCode,
            AccountType = transaction.Account.Type,
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category != null ? transaction.Category.Name : null,
            CategoryColor = transaction.Category != null ? transaction.Category.Color : null,
            OriginalAmount = transaction.OriginalAmount,
            OriginalCurrencyCode = transaction.OriginalCurrencyCode,
            ExchangeRate = transaction.ExchangeRate,
            OwnerId = transaction.OwnerId,
            OwnerLogin = transaction.Owner.Login
        };
    }

    public (Transaction? Transaction, decimal OldAmount, int OldAccountId) Create(int accountId, CreateTransactionDto transactionDto, int ownerId, out string? errorMessage)
    {
        errorMessage = null;
        decimal oldAmount = 0;
        int oldAccountId = 0;

        if (!_accountService.HasAccountAccess(accountId, ownerId, requireWriteAccess: true))
        {
            errorMessage = "Account not found or you do not have write access to it.";
            return (null, oldAmount, oldAccountId);
        }

        var accountCurrency = _accountService.GetAccountCurrency(accountId);
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
            AccountId = accountId,
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

    public (Transaction? Transaction, decimal OldAmount, int OldAccountId) Update(int transactionId, int accountId, UpdateTransactionDto transactionDto, int userId, out string? errorMessage)
    {
        errorMessage = null;
        var existingTransaction = _context.Transactions.Find(transactionId);
        if (existingTransaction == null)
        {
            errorMessage = "Transaction not found.";
            return (null, 0, 0);
        }

        if (existingTransaction.AccountId != accountId)
        {
            errorMessage = "Transaction does not belong to the specified account.";
            return (null, 0, 0);
        }

        if (existingTransaction.OwnerId != userId)
        {
            errorMessage = "You do not have permission to update this transaction.";
            return (null, 0, 0);
        }

        if (!_accountService.HasAccountAccess(accountId, userId, requireWriteAccess: true))
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

        var currentAccountCurrency = _accountService.GetAccountCurrency(existingTransaction.AccountId);

        if (transactionDto.OriginalCurrencyCode != null && transactionDto.OriginalCurrencyCode != currentAccountCurrency)
        {
            if (!transactionDto.OriginalAmount.HasValue || !transactionDto.ExchangeRate.HasValue || transactionDto.ExchangeRate.Value <= 0)
            {
                errorMessage = "Original amount and exchange rate are required for cross-currency transactions when changing currency.";
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

    public bool Delete(int transactionId, int accountId, int userId, out decimal oldAmount, out int oldAccountId)
    {
        oldAmount = 0;
        oldAccountId = 0;

        var transaction = _context.Transactions.Find(transactionId);
        if (transaction == null) return false;

        if (transaction.AccountId != accountId)
        {
            return false;
        }

        if (transaction.OwnerId != userId)
        {
            return false;
        }

        if (!_accountService.HasAccountAccess(accountId, userId, requireWriteAccess: true))
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
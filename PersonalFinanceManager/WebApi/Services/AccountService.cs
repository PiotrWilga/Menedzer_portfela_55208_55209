// Services/AccountService.cs
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;

    public AccountService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Account> GetAll(int userId)
    {
        return _context.Accounts
            .Include(a => a.Owner)
            .Include(a => a.AccountPermissions)
                .ThenInclude(ap => ap.AppUser)
            .Where(a => a.OwnerId == userId || a.AccountPermissions.Any(ap => ap.AppUserId == userId))
            .ToList();
    }

    public Account GetById(int id)
    {
        return _context.Accounts
            .Include(a => a.Owner)
            .Include(a => a.AccountPermissions)
                .ThenInclude(ap => ap.AppUser)
            .FirstOrDefault(a => a.Id == id);
    }

    public Account Create(CreateAccountDto accountDto, int ownerUserId)
    {
        var account = new Account
        {
            Name = accountDto.Name,
            Type = accountDto.Type,
            CurrencyCode = accountDto.CurrencyCode,
            Balance = accountDto.Balance,
            ShowInSummary = accountDto.ShowInSummary,
            OwnerId = ownerUserId
        };

        _context.Accounts.Add(account);
        _context.SaveChanges();
        return account;
    }

    public bool Update(int id, UpdateAccountDto updatedAccountDto, int userId)
    {
        var account = _context.Accounts
            .Include(a => a.AccountPermissions)
            .FirstOrDefault(a => a.Id == id);

        if (account == null) return false;

        if (account.OwnerId != userId &&
            !account.AccountPermissions.Any(ap => ap.AppUserId == userId && ap.PermissionType == PermissionType.ReadAndWrite))
        {
            return false;
        }

        account.Name = updatedAccountDto.Name ?? account.Name;
        account.Type = updatedAccountDto.Type ?? account.Type;
        account.CurrencyCode = updatedAccountDto.CurrencyCode ?? account.CurrencyCode;
        account.Balance = updatedAccountDto.Balance ?? account.Balance;
        account.ShowInSummary = updatedAccountDto.ShowInSummary ?? account.ShowInSummary;

        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id, int userId)
    {
        var account = _context.Accounts.Find(id);
        if (account == null) return false;

        if (account.OwnerId != userId)
        {
            return false;
        }

        _context.Accounts.Remove(account);
        _context.SaveChanges();
        return true;
    }

    public bool AddAccountPermission(int accountId, int userId, PermissionType permissionType)
    {
        var account = _context.Accounts.Find(accountId);
        var user = _context.AppUsers.Find(userId);

        if (account == null || user == null) return false;

        var existingPermission = _context.AccountPermissions
            .FirstOrDefault(ap => ap.AccountId == accountId && ap.AppUserId == userId);

        if (existingPermission != null)
        {
            existingPermission.PermissionType = permissionType;
        }
        else
        {
            var permission = new AccountPermission
            {
                AccountId = accountId,
                AppUserId = userId,
                PermissionType = permissionType
            };
            _context.AccountPermissions.Add(permission);
        }

        _context.SaveChanges();
        return true;
    }

    public bool RemoveAccountPermission(int accountId, int userId)
    {
        var permission = _context.AccountPermissions
            .FirstOrDefault(ap => ap.AccountId == accountId && ap.AppUserId == userId);

        if (permission == null) return false;

        _context.AccountPermissions.Remove(permission);
        _context.SaveChanges();
        return true;
    }

    // --- Nowe metody do zarządzania balansem konta ---
    public bool UpdateAccountBalance(int accountId, decimal amountChange)
    {
        var account = _context.Accounts.Find(accountId);
        if (account == null) return false;

        account.Balance += amountChange;
        _context.SaveChanges();
        return true;
    }

    public decimal GetAccountBalance(int accountId)
    {
        return _context.Accounts.Where(a => a.Id == accountId).Select(a => a.Balance).FirstOrDefault();
    }

    public string GetAccountCurrency(int accountId)
    {
        return _context.Accounts.Where(a => a.Id == accountId).Select(a => a.CurrencyCode).FirstOrDefault();
    }

    public bool HasAccountAccess(int accountId, int userId, bool requireWriteAccess = false)
    {
        var account = _context.Accounts
            .Include(a => a.AccountPermissions)
            .FirstOrDefault(a => a.Id == accountId);

        if (account == null) return false;

        // Właściciel zawsze ma pełny dostęp
        if (account.OwnerId == userId) return true;

        // Sprawdź uprawnienia współużytkownika
        var permission = account.AccountPermissions.FirstOrDefault(ap => ap.AppUserId == userId);
        if (permission == null) return false; // Brak uprawnień

        if (requireWriteAccess && permission.PermissionType == PermissionType.ReadOnly)
        {
            return false; // Wymagany zapis, ale użytkownik ma tylko odczyt
        }

        return true; // Ma odczyt lub odczyt/zapis zgodnie z wymaganiem
    }
}
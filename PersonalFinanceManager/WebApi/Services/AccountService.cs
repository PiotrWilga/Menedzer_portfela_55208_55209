using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace PersonalFinanceManager.WebApi.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;

    public AccountService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Account> GetAll() => _context.Accounts.ToList();

    public Account GetById(int id) => _context.Accounts.Find(id);

    public Account Create(Account account)
    {
        _context.Accounts.Add(account);
        _context.SaveChanges();
        return account;
    }

    public bool Update(int id, Account updatedAccount)
    {
        var account = _context.Accounts.Find(id);
        if (account == null) return false;

        account.Name = updatedAccount.Name;
        account.Type = updatedAccount.Type;
        account.CurrencyCode = updatedAccount.CurrencyCode;
        account.Balance = updatedAccount.Balance;
        account.ShowInSummary = updatedAccount.ShowInSummary;

        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var account = _context.Accounts.Find(id);
        if (account == null) return false;

        _context.Accounts.Remove(account);
        _context.SaveChanges();
        return true;
    }
}

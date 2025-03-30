using PersonalFinanceManager.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace PersonalFinanceManager.WebApi.Services;

public class AccountService : IAccountService
{
    private readonly List<Account> accounts = new List<Account>();

    public IEnumerable<Account> GetAll() => accounts;

    public Account GetById(int id) => accounts.FirstOrDefault(a => a.Id == id);

    public Account Create(Account account)
    {
        account.Id = accounts.Count + 1;
        accounts.Add(account);
        return account;
    }

    public bool Update(int id, Account updatedAccount)
    {
        var account = accounts.FirstOrDefault(a => a.Id == id);
        if (account == null) return false;

        account.Name = updatedAccount.Name;
        account.Type = updatedAccount.Type;
        account.CurrencyCode = updatedAccount.CurrencyCode;
        account.Balance = updatedAccount.Balance;
        account.ShowInSummary = updatedAccount.ShowInSummary;

        return true;
    }

    public bool Delete(int id)
    {
        var account = accounts.FirstOrDefault(a => a.Id == id);
        if (account == null) return false;

        accounts.Remove(account);
        return true;
    }
}
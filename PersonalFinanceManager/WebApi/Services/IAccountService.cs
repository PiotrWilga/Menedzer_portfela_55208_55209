using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Services;

public interface IAccountService
{
    IEnumerable<Account> GetAll();
    Account GetById(int id);
    Account Create(Account account);
    bool Update(int id, Account updatedAccount);
    bool Delete(int id);
}

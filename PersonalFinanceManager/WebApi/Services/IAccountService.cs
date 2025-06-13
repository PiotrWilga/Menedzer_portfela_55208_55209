using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Dtos; // Dodaj to

namespace PersonalFinanceManager.WebApi.Services;

public interface IAccountService
{
    IEnumerable<Account> GetAll(int userId);
    Account GetById(int id);
    Account Create(CreateAccountDto accountDto, int ownerUserId);
    bool Update(int id, UpdateAccountDto updatedAccountDto, int userId);
    bool Delete(int id, int userId); // Dodaj userId do usunięcia
    bool AddAccountPermission(int accountId, int userId, PermissionType permissionType);
    bool RemoveAccountPermission(int accountId, int userId);
}
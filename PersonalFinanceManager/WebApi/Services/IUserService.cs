using PersonalFinanceManager.WebApi.Dtos;
using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Services;

public interface IUserService
{
    Task<(bool Success, string? Error)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string? Token, AppUser? User, string? Error)> AuthenticateAsync(LoginDto dto);
}

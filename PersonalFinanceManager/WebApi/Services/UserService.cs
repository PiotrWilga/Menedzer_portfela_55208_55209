using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Dtos;
using PersonalFinanceManager.WebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PersonalFinanceManager.WebApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public UserService(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    private string GenerateJwtToken(AppUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "kOleeJny-kluCZ-SekREtny-w-ApliKAcyji");

        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _dbContext.AppUsers
            .FirstOrDefaultAsync(u => u.Login == dto.Login || u.Email == dto.Email);

        if (existingUser != null)
            return (false, "User with this login or email already exists.");

        var user = new AppUser
        {
            Login = dto.Login,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DefaultCurrency = dto.DefaultCurrency,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AppUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Token, AppUser? User, string? Error)> AuthenticateAsync(LoginDto dto)
    {
        var user = await _dbContext.AppUsers
            .SingleOrDefaultAsync(u => u.Login == dto.Login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (false, null, null, "Invalid login or password.");

        var token = GenerateJwtToken(user);

        return (true, token, user, null);
    }

    public async Task<(bool Success, UserDto? User, string? Error)> GetUserAsync(int id)
    {
        var user = await _dbContext.AppUsers.FindAsync(id);
        if (user == null)
            return (false, null, "User not found.");

        var dto = new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            DefaultCurrency = user.DefaultCurrency
        };

        return (true, dto, null);
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _dbContext.AppUsers.FindAsync(userId);
        if (user == null)
            return (false, "User not found.");

        user.Email = dto.Email ?? user.Email;
        user.DefaultCurrency = dto.DefaultCurrency ?? user.DefaultCurrency;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _dbContext.AppUsers.Update(user);
        await _dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(int userId)
    {
        var user = await _dbContext.AppUsers.FindAsync(userId);
        if (user == null)
            return (false, "User not found.");

        _dbContext.AppUsers.Remove(user);
        await _dbContext.SaveChangesAsync();

        return (true, null);
    }
}

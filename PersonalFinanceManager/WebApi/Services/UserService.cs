using BCrypt.Net;
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
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public UserService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterDto dto)
    {
        if (await _context.AppUsers.AnyAsync(u => u.Login == dto.Login || u.Email == dto.Email))
        {
            return (false, "Użytkownik o takim loginie lub adresie email już istnieje");
        }

        var user = new AppUser
        {
            Login = dto.Login,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DefaultCurrency = dto.DefaultCurrency
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Token, AppUser? User, string? Error)> AuthenticateAsync(LoginDto dto)
    {
        var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Login == dto.Login);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return (false, null, null, "Nieprawidłowy login lub hasło");
        }

        var token = GenerateJwtToken(user);
        return (true, token, user, null);
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _context.AppUsers.FindAsync(userId);
        if (user == null)
            return (false, "Użytkownik nie istnieje");

        if (!string.IsNullOrWhiteSpace(dto.Login) && dto.Login != user.Login)
        {
            if (await _context.AppUsers.AnyAsync(u => u.Login == dto.Login))
                return (false, "Login jest już zajęty");
            user.Login = dto.Login;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (await _context.AppUsers.AnyAsync(u => u.Email == dto.Email))
                return (false, "Email jest już zajęty");
            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        if (!string.IsNullOrWhiteSpace(dto.DefaultCurrency))
        {
            user.DefaultCurrency = dto.DefaultCurrency;
        }

        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(int userId)
    {
        var user = await _context.AppUsers.FindAsync(userId);
        if (user == null)
            return (false, "Użytkownik nie istnieje");

        _context.AppUsers.Remove(user);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    private string GenerateJwtToken(AppUser user)
    {
        var secret = _config["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

        var key = Encoding.UTF8.GetBytes(secret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

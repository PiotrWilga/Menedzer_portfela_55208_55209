using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersonalFinanceManager.WebApi.Services;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalFinanceManager.WebApi.Tests.Users;

public class UserServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly IConfiguration _configuration;

    public UserServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // izolat dla każdego testu
            .Options;

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Jwt:Secret", "very_secret_test_key_1234567890AbDcAbCd" }
            })
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_ShouldRegister_WhenNewUser()
    {
        using var context = new AppDbContext(_dbOptions);
        var service = new UserService(context, _configuration);

        var result = await service.RegisterAsync(new RegisterDto
        {
            Login = "testuser",
            Email = "test@example.com",
            Password = "password123",
            DefaultCurrency = "USD"
        });

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.Single(context.AppUsers);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenDuplicate()
    {
        using var context = new AppDbContext(_dbOptions);
        context.AppUsers.Add(new AppUser
        {
            Login = "user",
            Email = "existing@example.com",
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var service = new UserService(context, _configuration);

        var result = await service.RegisterAsync(new RegisterDto
        {
            Login = "user",
            Email = "new@example.com",
            Password = "pass"
        });

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnToken_WhenValidCredentials()
    {
        using var context = new AppDbContext(_dbOptions);
        var password = "securePassword";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new AppUser
        {
            Login = "authuser",
            Email = "auth@example.com",
            PasswordHash = hash
        };
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, _configuration);

        var result = await service.AuthenticateAsync(new LoginDto
        {
            Login = "authuser",
            Password = password
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.Equal(user.Login, result.User!.Login);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFail_WhenWrongPassword()
    {
        using var context = new AppDbContext(_dbOptions);
        context.AppUsers.Add(new AppUser
        {
            Login = "authfail",
            Email = "fail@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("realpass")
        });
        await context.SaveChangesAsync();

        var service = new UserService(context, _configuration);

        var result = await service.AuthenticateAsync(new LoginDto
        {
            Login = "authfail",
            Password = "wrongpass"
        });

        Assert.False(result.Success);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task GetUserAsync_ShouldReturnUser_WhenExists()
    {
        using var context = new AppDbContext(_dbOptions);
        var user = new AppUser
        {
            Login = "getme",
            Email = "getme@example.com",
            PasswordHash = "hash"
        };
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, _configuration);

        var result = await service.GetUserAsync(user.Id);

        Assert.True(result.Success);
        Assert.Equal(user.Login, result.User!.Login);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUser()
    {
        using var context = new AppDbContext(_dbOptions);
        var user = new AppUser
        {
            Login = "updateuser",
            Email = "old@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("old"),
            DefaultCurrency = "PLN"
        };
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, _configuration);

        var result = await service.UpdateUserAsync(user.Id, new UpdateUserDto
        {
            Email = "new@example.com",
            DefaultCurrency = "USD",
            Password = "newpass"
        });

        Assert.True(result.Success);

        var updated = await context.AppUsers.FindAsync(user.Id);
        Assert.Equal("new@example.com", updated!.Email);
        Assert.Equal("USD", updated.DefaultCurrency);
        Assert.True(BCrypt.Net.BCrypt.Verify("newpass", updated.PasswordHash));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldRemoveUser()
    {
        using var context = new AppDbContext(_dbOptions);
        var user = new AppUser
        {
            Login = "deleteuser",
            Email = "delete@example.com",
            PasswordHash = "hash"
        };
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        var service = new UserService(context, _configuration);

        var result = await service.DeleteUserAsync(user.Id);

        Assert.True(result.Success);
        Assert.Null(await context.AppUsers.FindAsync(user.Id));
    }
}

using Xunit;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using PersonalFinanceManager.WebApi.Dtos;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PersonalFinanceManager.WebApi.Tests.Accounts;

public class AccountServiceTests
{
    private DbContextOptions<AppDbContext> GetDbOptions() =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private AppUser CreateUser(string login, int id)
        => new AppUser { Id = id, Login = login, Email = $"{login}@test.com", PasswordHash = "hash" };

    [Fact]
    public void Create_AddsNewAccount()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser("owner", 1);
        context.AppUsers.Add(user);
        context.SaveChanges();

        var service = new AccountService(context);
        var account = service.Create(new CreateAccountDto
        {
            Name = "My Account",
            Type = AccountType.Cash,
            CurrencyCode = "PLN",
            Balance = 100,
            ShowInSummary = true
        }, ownerUserId: 1);

        Assert.NotNull(account);
        Assert.Equal("My Account", account.Name);
        Assert.Equal(1, context.Accounts.Count());
    }

    [Fact]
    public void GetAll_ReturnsOwnedAndPermittedAccounts()
    {
        using var context = new AppDbContext(GetDbOptions());
        var owner = CreateUser("owner", 1);
        var viewer = CreateUser("viewer", 2);
        var account1 = new Account { Name = "A1", CurrencyCode = "PLN", OwnerId = 1 };
        var account2 = new Account { Name = "A2", CurrencyCode = "USD", OwnerId = 3 };
        context.AppUsers.AddRange(owner, viewer);
        context.Accounts.AddRange(account1, account2);
        context.AccountPermissions.Add(new AccountPermission
        {
            Account = account2,
            AppUser = viewer,
            PermissionType = PermissionType.ReadOnly
        });
        context.SaveChanges();

        var service = new AccountService(context);
        var result = service.GetAll(2).ToList();

        Assert.Equal(1, result.Count); // viewer only sees A2
        Assert.Equal("A2", result[0].Name);
    }

    [Fact]
    public void GetById_ReturnsAccountWithOwner()
    {
        using var context = new AppDbContext(GetDbOptions());
        var owner = CreateUser("admin", 1);
        var account = new Account { Name = "Test", CurrencyCode = "USD", Owner = owner };
        context.Accounts.Add(account);
        context.SaveChanges();

        var service = new AccountService(context);
        var result = service.GetById(account.Id);

        Assert.NotNull(result);
        Assert.Equal("admin", result.OwnerLogin);
    }

    [Fact]
    public void Update_AllowedForOwner()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "Old", CurrencyCode = "EUR", OwnerId = 1 };
        context.Accounts.Add(account);
        context.SaveChanges();

        var service = new AccountService(context);
        var result = service.Update(account.Id, new UpdateAccountDto { Name = "Updated" }, 1);

        Assert.True(result);
        Assert.Equal("Updated", context.Accounts.Find(account.Id)!.Name);
    }

    [Fact]
    public void Update_AllowedForWritePermission()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "WithPermission", CurrencyCode = "EUR", OwnerId = 1 };
        var user = CreateUser("writer", 2);
        context.Accounts.Add(account);
        context.AppUsers.Add(user);
        context.AccountPermissions.Add(new AccountPermission
        {
            Account = account,
            AppUser = user,
            PermissionType = PermissionType.ReadAndWrite
        });
        context.SaveChanges();

        var service = new AccountService(context);
        var result = service.Update(account.Id, new UpdateAccountDto { Name = "Changed" }, 2);

        Assert.True(result);
        Assert.Equal("Changed", context.Accounts.Find(account.Id)!.Name);
    }

    [Fact]
    public void Update_DeniedForReadOnly()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "Secure", CurrencyCode = "EUR", OwnerId = 1 };
        var user = CreateUser("readonly", 2);
        context.Accounts.Add(account);
        context.AppUsers.Add(user);
        context.AccountPermissions.Add(new AccountPermission
        {
            Account = account,
            AppUser = user,
            PermissionType = PermissionType.ReadOnly
        });
        context.SaveChanges();

        var service = new AccountService(context);
        var result = service.Update(account.Id, new UpdateAccountDto { Name = "Hacked" }, 2);

        Assert.False(result);
        Assert.Equal("Secure", context.Accounts.Find(account.Id)!.Name);
    }

    [Fact]
    public void Delete_OnlyByOwner()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "ToDelete", CurrencyCode = "USD", OwnerId = 1 };
        context.Accounts.Add(account);
        context.SaveChanges();

        var service = new AccountService(context);
        var success = service.Delete(account.Id, 1);

        Assert.True(success);
        Assert.Empty(context.Accounts);
    }

    [Fact]
    public void AddAndRemovePermission_WorksCorrectly()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser("guest", 2);
        var account = new Account { Name = "Shared", CurrencyCode = "PLN", OwnerId = 1 };
        context.Accounts.Add(account);
        context.AppUsers.Add(user);
        context.SaveChanges();

        var service = new AccountService(context);
        var added = service.AddAccountPermission(account.Id, 2, PermissionType.ReadOnly);
        var removed = service.RemoveAccountPermission(account.Id, 2);

        Assert.True(added);
        Assert.True(removed);
    }

    [Fact]
    public void UpdateBalance_Works()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "Money", Balance = 50, CurrencyCode = "PLN", OwnerId = 1 };
        context.Accounts.Add(account);
        context.SaveChanges();

        var service = new AccountService(context);
        service.UpdateAccountBalance(account.Id, 25);

        Assert.Equal(75, context.Accounts.Find(account.Id)!.Balance);
    }

    [Fact]
    public void GetBalanceAndCurrency_Works()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "Wallet", Balance = 123.45m, CurrencyCode = "USD", OwnerId = 1 };
        context.Accounts.Add(account);
        context.SaveChanges();

        var service = new AccountService(context);
        Assert.Equal(123.45m, service.GetAccountBalance(account.Id));
        Assert.Equal("USD", service.GetAccountCurrency(account.Id));
    }

    [Fact]
    public void HasAccess_RespectsPermissionType()
    {
        using var context = new AppDbContext(GetDbOptions());
        var account = new Account { Name = "AccessTest", OwnerId = 1 };
        var user = CreateUser("permitted", 2);
        context.Accounts.Add(account);
        context.AppUsers.Add(user);
        context.AccountPermissions.Add(new AccountPermission
        {
            Account = account,
            AppUser = user,
            PermissionType = PermissionType.ReadOnly
        });
        context.SaveChanges();

        var service = new AccountService(context);
        Assert.True(service.HasAccountAccess(account.Id, 2)); // read
        Assert.False(service.HasAccountAccess(account.Id, 2, requireWriteAccess: true)); // no write
    }
}

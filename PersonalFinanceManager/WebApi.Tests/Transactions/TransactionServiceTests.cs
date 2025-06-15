using Xunit;
using Moq;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using PersonalFinanceManager.WebApi.Dtos;
using System.Collections.Generic;

namespace PersonalFinanceManager.WebApi.Tests.Transactions;

public class TransactionServiceTests
{
    private DbContextOptions<AppDbContext> GetDbOptions() =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private AppUser CreateUser(int id = 1) => new AppUser { Id = id, Login = "user", Email = "u@e.com" };

    [Fact]
    public void GetAll_ReturnsEmpty_WhenNoAccess()
    {
        using var context = new AppDbContext(GetDbOptions());
        var mockAccService = new Mock<IAccountService>();
        mockAccService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(false);

        var service = new TransactionService(context, mockAccService.Object);
        var result = service.GetAll(1, 42);

        Assert.Empty(result);
    }

    [Fact]
    public void GetAll_ReturnsUserTransactions()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser();
        var account = new Account { Id = 1, Name = "Main", OwnerId = user.Id, CurrencyCode = "PLN" };
        context.AppUsers.Add(user);
        context.Accounts.Add(account);
        context.Transactions.Add(new Transaction
        {
            Name = "Coffee",
            Amount = 10,
            Date = DateTime.UtcNow,
            OwnerId = user.Id,
            AccountId = 1
        });
        context.SaveChanges();

        var mockAccService = new Mock<IAccountService>();
        mockAccService.Setup(s => s.HasAccountAccess(1, user.Id, false)).Returns(true);

        var service = new TransactionService(context, mockAccService.Object);
        var result = service.GetAll(1, user.Id).ToList();

        Assert.Single(result);
        Assert.Equal("Coffee", result[0].Name);
    }

    [Fact]
    public void Create_StoresTransaction_WhenValid()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser();
        var account = new Account { Id = 1, CurrencyCode = "PLN", Balance = 0, OwnerId = user.Id };
        context.Accounts.Add(account);
        context.AppUsers.Add(user);
        context.SaveChanges();

        var mock = new Mock<IAccountService>();
        mock.Setup(s => s.HasAccountAccess(1, user.Id, true)).Returns(true);
        mock.Setup(s => s.GetAccountCurrency(1)).Returns("PLN");
        mock.Setup(s => s.UpdateAccountBalance(1, 100)).Returns(true);

        var service = new TransactionService(context, mock.Object);

        var dto = new CreateTransactionDto
        {
            Name = "Lunch",
            Amount = 100
        };

        var (tx, _, _) = service.Create(1, dto, user.Id, out var error);

        Assert.NotNull(tx);
        Assert.Equal(100, tx!.Amount);
        Assert.Null(error);
    }

    [Fact]
    public void Create_ReturnsError_WhenCurrencyMismatchAndMissingExchangeRate()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser();
        context.Accounts.Add(new Account { Id = 1, CurrencyCode = "PLN", OwnerId = user.Id });
        context.AppUsers.Add(user);
        context.SaveChanges();

        var mock = new Mock<IAccountService>();
        mock.Setup(s => s.HasAccountAccess(1, user.Id, true)).Returns(true);
        mock.Setup(s => s.GetAccountCurrency(1)).Returns("PLN");

        var service = new TransactionService(context, mock.Object);
        var dto = new CreateTransactionDto
        {
            Name = "Foreign Tx",
            OriginalCurrencyCode = "USD",
            OriginalAmount = 10
            // ExchangeRate is missing
        };

        var (tx, _, _) = service.Create(1, dto, user.Id, out var error);

        Assert.Null(tx);
        Assert.Equal("Original amount and exchange rate are required for cross-currency transactions.", error);
    }

    [Fact]
    public void Update_ChangesFields_WhenValid()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser();
        var account = new Account { Id = 1, CurrencyCode = "PLN", OwnerId = user.Id };
        var tx = new Transaction
        {
            Id = 5,
            Name = "Old",
            Amount = 50,
            AccountId = 1,
            OwnerId = user.Id
        };

        context.Accounts.Add(account);
        context.AppUsers.Add(user);
        context.Transactions.Add(tx);
        context.SaveChanges();

        var mock = new Mock<IAccountService>();
        mock.Setup(s => s.HasAccountAccess(1, user.Id, true)).Returns(true);
        mock.Setup(s => s.GetAccountCurrency(1)).Returns("PLN");
        mock.Setup(s => s.UpdateAccountBalance(It.IsAny<int>(), It.IsAny<decimal>())).Returns(true);

        var service = new TransactionService(context, mock.Object);

        var update = new UpdateTransactionDto
        {
            Name = "Updated",
            Amount = 100
        };

        var (updatedTx, oldAmt, oldAcc) = service.Update(5, 1, update, user.Id, out var error);

        Assert.NotNull(updatedTx);
        Assert.Equal("Updated", updatedTx!.Name);
        Assert.Equal(50, oldAmt);
        Assert.Equal(1, oldAcc);
        Assert.Null(error);
    }

    [Fact]
    public void Delete_RemovesTransaction_WhenAuthorized()
    {
        using var context = new AppDbContext(GetDbOptions());
        var user = CreateUser();
        var tx = new Transaction { Id = 1, Name = "DeleteMe", OwnerId = user.Id, AccountId = 1, Amount = 10 };
        context.Transactions.Add(tx);
        context.AppUsers.Add(user);
        context.Accounts.Add(new Account { Id = 1, CurrencyCode = "PLN", OwnerId = user.Id });
        context.SaveChanges();

        var mock = new Mock<IAccountService>();
        mock.Setup(s => s.HasAccountAccess(1, user.Id, true)).Returns(true);
        mock.Setup(s => s.UpdateAccountBalance(1, -10)).Returns(true);

        var service = new TransactionService(context, mock.Object);
        var success = service.Delete(1, 1, user.Id, out var oldAmount, out var oldAccountId);

        Assert.True(success);
        Assert.Equal(10, oldAmount);
        Assert.Equal(1, oldAccountId);
        Assert.Empty(context.Transactions);
    }

    [Fact]
    public void Delete_Fails_WhenUnauthorized()
    {
        using var context = new AppDbContext(GetDbOptions());
        context.Transactions.Add(new Transaction { Id = 1, Name = "Private", OwnerId = 99, AccountId = 1 });
        context.SaveChanges();

        var mock = new Mock<IAccountService>();
        mock.Setup(s => s.HasAccountAccess(1, 42, true)).Returns(false);

        var service = new TransactionService(context, mock.Object);
        var result = service.Delete(1, 1, 42, out var _, out var _);

        Assert.False(result);
    }
}

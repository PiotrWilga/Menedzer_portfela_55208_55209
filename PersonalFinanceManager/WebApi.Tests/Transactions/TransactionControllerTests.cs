using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using PersonalFinanceManager.WebApi.Controllers;
using PersonalFinanceManager.WebApi.Services;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Tests.Transactions;

public class TransactionsControllerTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly TransactionsController _controller;

    public TransactionsControllerTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _mockAccountService = new Mock<IAccountService>();
        _controller = new TransactionsController(_mockTransactionService.Object, _mockAccountService.Object);
        SetUserContext(42);
    }

    private void SetUserContext(int userId)
    {
        var claims = new List<Claim> { new Claim("id", userId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public void GetAll_ReturnsOk_WhenAuthorized()
    {
        _mockAccountService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(true);
        _mockTransactionService.Setup(s => s.GetAll(1, 42)).Returns(new List<TransactionDto>
        {
            new TransactionDto { Id = 1, Name = "Test", OwnerId = 42, AccountId = 1 }
        });

        var result = _controller.GetAll(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<IEnumerable<TransactionDto>>(ok.Value);
        Assert.Single(data);
    }

    [Fact]
    public void GetAll_ReturnsForbid_WhenUnauthorized()
    {
        _mockAccountService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(false);

        var result = _controller.GetAll(1);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenValid()
    {
        var dto = new TransactionDto { Id = 1, AccountId = 1, OwnerId = 42 };
        _mockAccountService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(true);
        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns(dto);

        var result = _controller.GetById(1, 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenNull()
    {
        _mockAccountService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(true);
        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns((TransactionDto)null);

        var result = _controller.GetById(1, 1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsForbid_WhenOwnerMismatch()
    {
        var dto = new TransactionDto { Id = 1, AccountId = 1, OwnerId = 999 };
        _mockAccountService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(true);
        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns(dto);

        var result = _controller.GetById(1, 1);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenWrongAccount()
    {
        var dto = new TransactionDto { Id = 1, AccountId = 99, OwnerId = 42 };
        _mockAccountService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(true);
        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns(dto);

        var result = _controller.GetById(1, 1);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Transaction does not belong to the specified account.", notFound.Value);
    }

    [Fact]
    public void Create_ReturnsCreated_WhenSuccessful()
    {
        var dto = new CreateTransactionDto { Name = "Coffee", Amount = 10 };
        var tx = new Transaction { Id = 1, AccountId = 1 };
        var dtoOut = new TransactionDto { Id = 1, AccountId = 1, OwnerId = 42 };

        string? outErr = null;
        _mockTransactionService
            .Setup(s => s.Create(1, dto, 42, out outErr))
            .Returns((tx, 0m, 0));

        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns(dtoOut);

        var result = _controller.Create(1, dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(dtoOut, created.Value);
    }

    [Fact]
    public void Create_ReturnsBadRequest_WhenError()
    {
        var dto = new CreateTransactionDto { Name = "Invalid" };
        string? outErr = "Something went wrong";

        _mockTransactionService
            .Setup(s => s.Create(1, dto, 42, out outErr))
            .Returns((null, 0m, 0));

        var result = _controller.Create(1, dto);

        var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Something went wrong", badReq.Value!.ToString());
    }

    [Fact]
    public void Update_ReturnsNoContent_WhenSuccessful()
    {
        var dto = new UpdateTransactionDto { Name = "Updated" };

        string? outErr = null;
        _mockTransactionService
            .Setup(s => s.Update(1, 1, dto, 42, out outErr))
            .Returns((new Transaction(), 10m, 1));

        var result = _controller.Update(1, 1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_ReturnsBadRequest_WhenError()
    {
        var dto = new UpdateTransactionDto { Name = "Updated" };
        string? outErr = "No access";

        _mockTransactionService
            .Setup(s => s.Update(1, 1, dto, 42, out outErr))
            .Returns((null, 0, 0));

        var result = _controller.Update(1, 1, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("No access", bad.Value!.ToString());
    }

    [Fact]
    public void Delete_ReturnsNoContent_WhenSuccessful()
    {
        decimal oldAmount = 0;
        int oldAccountId = 0;

        _mockTransactionService
            .Setup(s => s.Delete(1, 1, 42, out oldAmount, out oldAccountId))
            .Returns(true);

        var result = _controller.Delete(1, 1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenNotExists()
    {
        decimal oldAmount = 0;
        int oldAccountId = 0;

        _mockTransactionService
            .Setup(s => s.Delete(1, 1, 42, out oldAmount, out oldAccountId))
            .Returns(false);

        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns((TransactionDto)null);

        var result = _controller.Delete(1, 1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ReturnsForbid_WhenDenied()
    {
        decimal oldAmount = 0;
        int oldAccountId = 0;
        var dto = new TransactionDto { Id = 1, OwnerId = 999, AccountId = 1 };

        _mockTransactionService
            .Setup(s => s.Delete(1, 1, 42, out oldAmount, out oldAccountId))
            .Returns(false);

        _mockTransactionService.Setup(s => s.GetById(1, 1)).Returns(dto);

        var result = _controller.Delete(1, 1);

        Assert.IsType<ForbidResult>(result);
    }
}

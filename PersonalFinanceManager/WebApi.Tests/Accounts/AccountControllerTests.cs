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

namespace PersonalFinanceManager.WebApi.Tests.Accounts;
public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _mockService;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mockService = new Mock<IAccountService>();
        _controller = new AccountsController(_mockService.Object);
        SetUserContext(42);
    }

    private void SetUserContext(int userId)
    {
        var claims = new List<Claim> { new Claim("id", userId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public void GetAll_ReturnsOkWithAccounts()
    {
        _mockService.Setup(s => s.GetAll(42))
            .Returns(new List<AccountDto> { new AccountDto { Id = 1, Name = "Test" } });

        var result = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<IEnumerable<AccountDto>>(ok.Value);
        Assert.Single(data);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenAuthorized()
    {
        var account = new AccountDto { Id = 1, OwnerId = 42 };
        _mockService.Setup(s => s.GetById(1)).Returns(account);
        _mockService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(true);

        var result = _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<AccountDto>(ok.Value);
        Assert.Equal(1, value.Id);
    }

    [Fact]
    public void GetById_ReturnsForbid_WhenNoAccess()
    {
        var account = new AccountDto { Id = 1, OwnerId = 99 };
        _mockService.Setup(s => s.GetById(1)).Returns(account);
        _mockService.Setup(s => s.HasAccountAccess(1, 42, false)).Returns(false);

        var result = _controller.GetById(1);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenNull()
    {
        _mockService.Setup(s => s.GetById(1)).Returns((AccountDto)null);

        var result = _controller.GetById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_ReturnsCreatedAt()
    {
        var dto = new CreateAccountDto { Name = "New", CurrencyCode = "PLN" };
        var createdAccount = new Account { Id = 99, Name = "New", OwnerId = 42 };
        var returnedDto = new AccountDto { Id = 99, Name = "New", OwnerId = 42 };

        _mockService.Setup(s => s.Create(dto, 42)).Returns(createdAccount);
        _mockService.Setup(s => s.GetById(99)).Returns(returnedDto);

        var result = _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<AccountDto>(created.Value);
        Assert.Equal(99, value.Id);
        Assert.Equal("New", value.Name);
    }

    [Fact]
    public void Update_ReturnsNoContent_WhenSuccess()
    {
        var dto = new UpdateAccountDto { Name = "Updated" };
        _mockService.Setup(s => s.Update(1, dto, 42)).Returns(true);

        var result = _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenAccountMissing()
    {
        var dto = new UpdateAccountDto { Name = "Updated" };
        _mockService.Setup(s => s.Update(1, dto, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns((AccountDto)null);

        var result = _controller.Update(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Update_ReturnsForbid_WhenUnauthorized()
    {
        var dto = new UpdateAccountDto { Name = "Updated" };
        var account = new AccountDto { Id = 1, OwnerId = 999 };

        _mockService.Setup(s => s.Update(1, dto, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns(account);

        var result = _controller.Update(1, dto);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void Delete_ReturnsNoContent_WhenSuccess()
    {
        _mockService.Setup(s => s.Delete(1, 42)).Returns(true);

        var result = _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenMissing()
    {
        _mockService.Setup(s => s.Delete(1, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns((AccountDto)null);

        var result = _controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ReturnsForbid_WhenUnauthorized()
    {
        var acc = new AccountDto { Id = 1, OwnerId = 99 };
        _mockService.Setup(s => s.Delete(1, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns(acc);

        var result = _controller.Delete(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void AddAccountPermission_ReturnsNoContent_WhenOwner()
    {
        var dto = new AddAccountPermissionDto { AppUserId = 7, PermissionType = PermissionType.ReadOnly };
        var account = new AccountDto { Id = 1, OwnerId = 42 };

        _mockService.Setup(s => s.GetById(1)).Returns(account);
        _mockService.Setup(s => s.AddAccountPermission(1, 7, PermissionType.ReadOnly)).Returns(true);

        var result = _controller.AddAccountPermission(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void AddAccountPermission_ReturnsForbid_WhenNotOwner()
    {
        var dto = new AddAccountPermissionDto { AppUserId = 7, PermissionType = PermissionType.ReadOnly };
        var account = new AccountDto { Id = 1, OwnerId = 99 };

        _mockService.Setup(s => s.GetById(1)).Returns(account);

        var result = _controller.AddAccountPermission(1, dto);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void RemoveAccountPermission_ReturnsNoContent_WhenSuccess()
    {
        var acc = new AccountDto { Id = 1, OwnerId = 42 };
        _mockService.Setup(s => s.GetById(1)).Returns(acc);
        _mockService.Setup(s => s.RemoveAccountPermission(1, 77)).Returns(true);

        var result = _controller.RemoveAccountPermission(1, 77);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void RemoveAccountPermission_ReturnsNotFound_WhenMissing()
    {
        var acc = new AccountDto { Id = 1, OwnerId = 42 };
        _mockService.Setup(s => s.GetById(1)).Returns(acc);
        _mockService.Setup(s => s.RemoveAccountPermission(1, 77)).Returns(false);

        var result = _controller.RemoveAccountPermission(1, 77);

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Account permission not found.", nf.Value);
    }
}

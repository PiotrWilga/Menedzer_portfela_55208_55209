using Xunit;
using Moq;
using PersonalFinanceManager.WebApi.Controllers;
using PersonalFinanceManager.WebApi.Services;
using PersonalFinanceManager.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PersonalFinanceManager.WebApi.Models; // Upewnij się, że User jest dostępny
using PersonalFinanceManager.WebApi.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PersonalFinanceManager.WebApi.Tests.Users;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new UsersController(_mockUserService.Object);
    }

    [Fact]
    public async Task Register_ReturnsStatusCode201_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var registerDto = new RegisterDto { Login = "testuser", Email = "test@example.com", Password = "Password123!" };
        // Serwis zwraca krotkę (Success: true, Error: null)
        _mockUserService.Setup(s => s.RegisterAsync(registerDto))
            .ReturnsAsync((true, (string?)null)); // Changed here

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsType<StatusCodeResult>(result);
        var statusCodeResult = result as StatusCodeResult;
        Assert.Equal(201, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var registerDto = new RegisterDto { Login = "testuser", Email = "test@example.com", Password = "Password123!" };
        var errorMessage = "User already exists.";
        // Serwis zwraca krotkę (Success: false, Error: "User already exists.")
        _mockUserService.Setup(s => s.RegisterAsync(registerDto))
            .ReturnsAsync((false, errorMessage)); // Changed here

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
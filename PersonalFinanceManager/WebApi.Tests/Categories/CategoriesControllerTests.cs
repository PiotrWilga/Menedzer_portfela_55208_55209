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

namespace PersonalFinanceManager.WebApi.Tests.Categories;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _mockService;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockService = new Mock<ICategoryService>();
        _controller = new CategoriesController(_mockService.Object);
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
    public void GetAll_ReturnsUserCategories()
    {
        _mockService.Setup(s => s.GetAll(42))
            .Returns(new List<Category> { new Category { Id = 1, Name = "Food", OwnerId = 42 } });

        var result = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(ok.Value);
        Assert.Single(categories);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenOwner()
    {
        var category = new Category { Id = 1, Name = "Travel", OwnerId = 42 };
        _mockService.Setup(s => s.GetById(1)).Returns(category);

        var result = _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<Category>(ok.Value);
        Assert.Equal("Travel", value.Name);
    }

    [Fact]
    public void GetById_ReturnsForbid_WhenNotOwner()
    {
        var category = new Category { Id = 1, OwnerId = 999 };
        _mockService.Setup(s => s.GetById(1)).Returns(category);

        var result = _controller.GetById(1);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenNull()
    {
        _mockService.Setup(s => s.GetById(1)).Returns((Category)null);

        var result = _controller.GetById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Create_ReturnsCreatedCategory()
    {
        var dto = new CreateCategoryDto { Name = "New", Color = "#fff", Description = "desc" };
        var created = new Category { Id = 5, Name = "New", OwnerId = 42 };

        _mockService.Setup(s => s.Create(dto, 42)).Returns(created);

        var result = _controller.Create(dto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var value = Assert.IsType<Category>(createdAt.Value);
        Assert.Equal("New", value.Name);
        Assert.Equal(5, value.Id);
    }

    [Fact]
    public void Update_ReturnsNoContent_WhenSuccess()
    {
        var dto = new UpdateCategoryDto { Name = "Updated" };
        _mockService.Setup(s => s.Update(1, dto, 42)).Returns(true);

        var result = _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenCategoryMissing()
    {
        var dto = new UpdateCategoryDto { Name = "X" };
        _mockService.Setup(s => s.Update(1, dto, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns((Category)null);

        var result = _controller.Update(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Update_ReturnsForbid_WhenNotOwner()
    {
        var dto = new UpdateCategoryDto { Name = "X" };
        var category = new Category { Id = 1, OwnerId = 999 };

        _mockService.Setup(s => s.Update(1, dto, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns(category);

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
    public void Delete_ReturnsNotFound_WhenCategoryNotFound()
    {
        _mockService.Setup(s => s.Delete(1, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns((Category)null);

        var result = _controller.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ReturnsForbid_WhenNotOwner()
    {
        var cat = new Category { Id = 1, OwnerId = 999 };
        _mockService.Setup(s => s.Delete(1, 42)).Returns(false);
        _mockService.Setup(s => s.GetById(1)).Returns(cat);

        var result = _controller.Delete(1);

        Assert.IsType<ForbidResult>(result);
    }
}

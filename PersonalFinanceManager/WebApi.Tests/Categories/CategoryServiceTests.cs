using Xunit;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using PersonalFinanceManager.WebApi.Dtos;
using System.Linq;
using System.Collections.Generic;

namespace PersonalFinanceManager.WebApi.Tests.Categories;
public class CategoryServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"InMemoryDb_{Guid.NewGuid()}") // Użyj unikalnej nazwy bazy danych
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureDeleted(); // Zapewnia czystą bazę danych dla każdego testu
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void GetAll_ShouldReturnCategoriesForGivenUser()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var userService = new CategoryService(context);

        var user1Id = 1;
        var user2Id = 2;

        context.AppUsers.Add(new AppUser { Id = user1Id, Login = "login" });
        context.AppUsers.Add(new AppUser { Id = user2Id, Login = "Login" });
        context.SaveChanges();

        context.Categories.Add(new Category { Id = 1, Name = "Food", Color = "#FF0000", OwnerId = user1Id });
        context.Categories.Add(new Category { Id = 2, Name = "Transport", Color = "#00FF00", OwnerId = user1Id });
        context.Categories.Add(new Category { Id = 3, Name = "Bills", Color = "#0000FF", OwnerId = user2Id });
        context.SaveChanges();

        // Act
        var categories = userService.GetAll(user1Id).ToList();

        // Assert
        Assert.NotNull(categories);
        Assert.Equal(2, categories.Count);
        Assert.True(categories.All(c => c.OwnerId == user1Id));
        Assert.Contains(categories, c => c.Name == "Food");
        Assert.Contains(categories, c => c.Name == "Transport");
    }

    [Fact]
    public void GetById_ShouldReturnCorrectCategory()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        var user1Id = 1;
        context.AppUsers.Add(new AppUser { Id = user1Id, Login = "login" });
        context.SaveChanges();

        context.Categories.Add(new Category { Id = 10, Name = "Shopping", Color = "#AAAAAA", OwnerId = user1Id });
        context.SaveChanges();

        // Act
        var category = categoryService.GetById(10);

        // Assert
        Assert.NotNull(category);
        Assert.Equal(10, category.Id);
        Assert.Equal("Shopping", category.Name);
    }

    [Fact]
    public void GetById_ShouldReturnNull_WhenCategoryNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        // Act
        var category = categoryService.GetById(999);

        // Assert
        Assert.Null(category);
    }

    [Fact]
    public void Create_ShouldAddCategoryToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        var ownerId = 1;
        context.AppUsers.Add(new AppUser { Id = ownerId, Login = "login" });
        context.SaveChanges();

        var newCategoryDto = new CreateCategoryDto
        {
            Name = "New Category",
            Color = "#123456",
            Description = "A description for the new category"
        };

        // Act
        var createdCategory = categoryService.Create(newCategoryDto, ownerId);

        // Assert
        Assert.NotNull(createdCategory);
        Assert.True(createdCategory.Id > 0); // Sprawdź, czy ID zostało wygenerowane
        Assert.Equal(newCategoryDto.Name, createdCategory.Name);
        Assert.Equal(newCategoryDto.Color, createdCategory.Color);
        Assert.Equal(newCategoryDto.Description, createdCategory.Description);
        Assert.Equal(ownerId, createdCategory.OwnerId);

        var categoryInDb = context.Categories.Find(createdCategory.Id);
        Assert.NotNull(categoryInDb);
        Assert.Equal(newCategoryDto.Name, categoryInDb.Name);
    }

    [Fact]
    public void Update_ShouldUpdateCategoryDetails_WhenOwnerMatches()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        var ownerId = 1;
        context.AppUsers.Add(new AppUser { Id = ownerId, Login = "login" });
        context.SaveChanges();

        var existingCategory = new Category { Id = 1, Name = "Old Name", Color = "#000000", Description = "Old Description", OwnerId = ownerId };
        context.Categories.Add(existingCategory);
        context.SaveChanges();

        var updatedCategoryDto = new UpdateCategoryDto
        {
            Name = "Updated Name",
            Color = "#FFFFFF",
            Description = "Updated Description"
        };

        // Act
        var result = categoryService.Update(existingCategory.Id, updatedCategoryDto, ownerId);

        // Assert
        Assert.True(result);
        var categoryInDb = context.Categories.Find(existingCategory.Id);
        Assert.NotNull(categoryInDb);
        Assert.Equal(updatedCategoryDto.Name, categoryInDb.Name);
        Assert.Equal(updatedCategoryDto.Color, categoryInDb.Color);
        Assert.Equal(updatedCategoryDto.Description, categoryInDb.Description);
    }

    [Fact]
    public void Update_ShouldReturnFalse_WhenCategoryNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);
        var updatedCategoryDto = new UpdateCategoryDto { Name = "NonExistent" };

        // Act
        var result = categoryService.Update(999, updatedCategoryDto, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Update_ShouldReturnFalse_WhenOwnerDoesNotMatch()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        var ownerId = 1;
        var anotherUserId = 2;
        context.AppUsers.Add(new AppUser { Id = ownerId, Login = "login" });
        context.AppUsers.Add(new AppUser { Id = anotherUserId, Login = "AnotherUser" });
        context.SaveChanges();

        var existingCategory = new Category { Id = 1, Name = "Owned Category", Color = "#ABCDEF", OwnerId = ownerId };
        context.Categories.Add(existingCategory);
        context.SaveChanges();

        var updatedCategoryDto = new UpdateCategoryDto { Name = "Attempted Update" };

        // Act
        var result = categoryService.Update(existingCategory.Id, updatedCategoryDto, anotherUserId);

        // Assert
        Assert.False(result);
        var categoryInDb = context.Categories.Find(existingCategory.Id);
        Assert.Equal("Owned Category", categoryInDb.Name); // Nazwa nie powinna zostać zmieniona
    }

    [Fact]
    public void Delete_ShouldRemoveCategory_WhenOwnerMatches()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        var ownerId = 1;
        context.AppUsers.Add(new AppUser { Id = ownerId, Login = "login" });
        context.SaveChanges();

        var categoryToDelete = new Category { Id = 1, Name = "To Delete", Color = "#111222", OwnerId = ownerId };
        context.Categories.Add(categoryToDelete);
        context.SaveChanges();

        // Act
        var result = categoryService.Delete(categoryToDelete.Id, ownerId);

        // Assert
        Assert.True(result);
        var categoryInDb = context.Categories.Find(categoryToDelete.Id);
        Assert.Null(categoryInDb);
    }

    [Fact]
    public void Delete_ShouldReturnFalse_WhenCategoryNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        // Act
        var result = categoryService.Delete(999, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Delete_ShouldReturnFalse_WhenOwnerDoesNotMatch()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var categoryService = new CategoryService(context);

        var ownerId = 1;
        var anotherUserId = 2;
        context.AppUsers.Add(new AppUser { Id = ownerId, Login = "Owner" });
        context.AppUsers.Add(new AppUser { Id = anotherUserId, Login = "login" });
        context.SaveChanges();

        var categoryToDelete = new Category { Id = 1, Name = "Owned By Others", Color = "#333444", OwnerId = ownerId };
        context.Categories.Add(categoryToDelete);
        context.SaveChanges();

        // Act
        var result = categoryService.Delete(categoryToDelete.Id, anotherUserId);

        // Assert
        Assert.False(result);
        var categoryInDb = context.Categories.Find(categoryToDelete.Id);
        Assert.NotNull(categoryInDb); // Kategoria nie powinna zostać usunięta
    }
}
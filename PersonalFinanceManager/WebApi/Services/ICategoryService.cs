// Services/ICategoryService.cs
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Services;

public interface ICategoryService
{
    IEnumerable<Category> GetAll(int userId);
    Category GetById(int id);
    Category Create(CreateCategoryDto categoryDto, int ownerUserId);
    bool Update(int id, UpdateCategoryDto updatedCategoryDto, int userId);
    bool Delete(int id, int userId);
}
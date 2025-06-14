using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Category> GetAll(int userId)
    {
        return _context.Categories
                       .Where(c => c.OwnerId == userId)
                       .ToList();
    }

    public Category GetById(int id)
    {
        return _context.Categories.FirstOrDefault(c => c.Id == id);
    }

    public Category Create(CreateCategoryDto categoryDto, int ownerUserId)
    {
        var category = new Category
        {
            Name = categoryDto.Name,
            Color = categoryDto.Color,
            OwnerId = ownerUserId
        };

        _context.Categories.Add(category);
        _context.SaveChanges();
        return category;
    }

    public bool Update(int id, UpdateCategoryDto updatedCategoryDto, int userId)
    {
        var category = _context.Categories.Find(id);
        if (category == null) return false;

        if (category.OwnerId != userId)
        {
            return false;
        }

        category.Name = updatedCategoryDto.Name ?? category.Name;
        category.Color = updatedCategoryDto.Color ?? category.Color;

        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id, int userId)
    {
        var category = _context.Categories.Find(id);
        if (category == null) return false;

        if (category.OwnerId != userId)
        {
            return false;
        }

        _context.Categories.Remove(category);
        _context.SaveChanges();
        return true;
    }
}
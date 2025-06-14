using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }


    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User ID claim not found.");
        }
        return int.Parse(userIdClaim);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Category>> GetAll()
    {
        var userId = GetUserIdFromClaims();
        var categories = _categoryService.GetAll(userId);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public ActionResult<Category> GetById(int id)
    {
        var userId = GetUserIdFromClaims();
        var category = _categoryService.GetById(id);

        if (category == null) return NotFound();


        if (category.OwnerId != userId)
        {
            return Forbid();
        }

        return Ok(category);
    }

    [HttpPost]
    public ActionResult<Category> Create([FromBody] CreateCategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerId = GetUserIdFromClaims();
        var createdCategory = _categoryService.Create(categoryDto, ownerId);
        return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateCategoryDto updatedCategoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserIdFromClaims();
        var result = _categoryService.Update(id, updatedCategoryDto, userId);
        if (!result)
        {
            var category = _categoryService.GetById(id);
            if (category == null) return NotFound();
            return Forbid();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = GetUserIdFromClaims();
        var result = _categoryService.Delete(id, userId);
        if (!result)
        {
            var category = _categoryService.GetById(id);
            if (category == null) return NotFound();
            return Forbid();
        }
        return NoContent();
    }
}
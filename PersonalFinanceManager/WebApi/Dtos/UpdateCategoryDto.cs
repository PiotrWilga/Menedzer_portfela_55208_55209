using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.WebApi.Dtos;

public class UpdateCategoryDto
{
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Category name cannot contain only whitespace.")]
    public string? Name { get; set; }

    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be in hexadecimal format (e.g., #RRGGBB or #RGB).")]
    public string? Color { get; set; }
}
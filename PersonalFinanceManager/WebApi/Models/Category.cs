// Models/Category.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceManager.WebApi.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters long.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Category name cannot contain only whitespace.")]
    public string Name { get; set; }

    [Required]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be in hexadecimal format (e.g., #RRGGBB or #RGB).")]
    public string Color { get; set; }

    public int OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public AppUser Owner { get; set; }
}
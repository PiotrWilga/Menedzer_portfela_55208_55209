using System.ComponentModel.DataAnnotations;
using PersonalFinanceManager.WebApi.Models;

namespace PersonalFinanceManager.WebApi.Dtos;

public class AddAccountPermissionDto
{
    [Required]
    public int AppUserId { get; set; }

    [Required]
    public PermissionType PermissionType { get; set; }
}
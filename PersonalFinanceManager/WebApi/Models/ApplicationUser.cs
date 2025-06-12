using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(8)]
    public string DefaultCurrency { get; set; }
}

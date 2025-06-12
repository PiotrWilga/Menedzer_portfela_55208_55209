using System.ComponentModel.DataAnnotations;

namespace WebApi.Requests;

public class UserRegister
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    [StringLength(8, ErrorMessage = "The DefaultCurrency length must be between {0} and {1}", MinimumLength = 2)]
    public string DefaultCurrenctyCode { get; set; }
}

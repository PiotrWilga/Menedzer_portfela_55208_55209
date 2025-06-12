namespace PersonalFinanceManager.WebApi.Dtos;

public class RegisterDto
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "PLN";
}

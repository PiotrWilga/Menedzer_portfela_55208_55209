namespace PersonalFinanceManager.WebApi.Dtos;

public class UpdateUserDto
{
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? DefaultCurrency { get; set; }
}

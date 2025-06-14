namespace PersonalFinanceManager.WebApi.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string? DefaultCurrency { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Dtos;
using PersonalFinanceManager.WebApi.Services;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _userService.RegisterAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _userService.AuthenticateAsync(dto);
        if (!result.Success)
            return Unauthorized(new { message = result.Error });

        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User!.Id,
                login = result.User.Login,
                email = result.User.Email,
                defaultCurrency = result.User.DefaultCurrency
            }
        });
    }
}

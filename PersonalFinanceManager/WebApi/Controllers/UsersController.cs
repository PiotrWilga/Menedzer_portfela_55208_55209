using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Dtos;
using PersonalFinanceManager.WebApi.Services;
using System.Security.Claims;

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
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _userService.RegisterAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return StatusCode(201);
    }

    [HttpPost("login")]
    [AllowAnonymous]
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

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null || int.Parse(userIdClaim) != id)
            return Forbid();

        var result = await _userService.UpdateUserAsync(id, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null || int.Parse(userIdClaim) != id)
            return Forbid();

        var result = await _userService.DeleteUserAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.Error });

        return NoContent();
    }
}

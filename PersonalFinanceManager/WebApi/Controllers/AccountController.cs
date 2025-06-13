using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User ID claim not found.");
        }
        return int.Parse(userIdClaim);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Account>> GetAll()
    {
        var userId = GetUserIdFromClaims();
        var accounts = _accountService.GetAll(userId);
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public ActionResult<Account> GetById(int id)
    {
        var userId = GetUserIdFromClaims();
        var account = _accountService.GetById(id);

        if (account == null) return NotFound();

        if (account.OwnerId != userId &&
            !account.AccountPermissions.Any(ap => ap.AppUserId == userId))
        {
            return Forbid();
        }

        return Ok(account);
    }

    [HttpPost]
    public ActionResult<Account> Create([FromBody] CreateAccountDto accountDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerId = GetUserIdFromClaims();
        var createdAccount = _accountService.Create(accountDto, ownerId);
        return CreatedAtAction(nameof(GetById), new { id = createdAccount.Id }, createdAccount);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateAccountDto updatedAccountDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserIdFromClaims();
        var result = _accountService.Update(id, updatedAccountDto, userId);
        if (!result)
        {
            var account = _accountService.GetById(id);
            if (account == null) return NotFound();
            return Forbid();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = GetUserIdFromClaims();
        var result = _accountService.Delete(id, userId);
        if (!result)
        {
            var account = _accountService.GetById(id);
            if (account == null) return NotFound();
            return Forbid();
        }
        return NoContent();
    }

    [HttpPost("{accountId}/permissions")]
    public IActionResult AddAccountPermission(int accountId, [FromBody] AddAccountPermissionDto dto)
    {
        var currentUserId = GetUserIdFromClaims();
        var account = _accountService.GetById(accountId);

        if (account == null) return NotFound("Account not found.");

        if (account.OwnerId != currentUserId)
        {
            return Forbid("Only the account owner can manage permissions.");
        }

        if (!_accountService.AddAccountPermission(accountId, dto.AppUserId, dto.PermissionType))
        {
            return BadRequest("Could not add account permission. User or Account not found, or permission already exists.");
        }

        return NoContent();
    }

    [HttpDelete("{accountId}/permissions/{appUserId}")]
    public IActionResult RemoveAccountPermission(int accountId, int appUserId)
    {
        var currentUserId = GetUserIdFromClaims();
        var account = _accountService.GetById(accountId);

        if (account == null) return NotFound("Account not found.");

        if (account.OwnerId != currentUserId)
        {
            return Forbid("Only the account owner can manage permissions.");
        }

        if (!_accountService.RemoveAccountPermission(accountId, appUserId))
        {
            return NotFound("Account permission not found.");
        }

        return NoContent();
    }
}
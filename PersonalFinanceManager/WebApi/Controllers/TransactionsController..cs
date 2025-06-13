// Controllers/TransactionsController.cs
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService; // Potrzebne do sprawdzenia dostępu do konta

    public TransactionsController(ITransactionService transactionService, IAccountService accountService)
    {
        _transactionService = transactionService;
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
    public ActionResult<IEnumerable<Transaction>> GetAll([FromQuery] int? accountId = null)
    {
        var userId = GetUserIdFromClaims();

        // Jeśli podano accountId, sprawdź, czy użytkownik ma do niego dostęp
        if (accountId.HasValue && !_accountService.HasAccountAccess(accountId.Value, userId))
        {
            return Forbid("You do not have access to the specified account.");
        }

        var transactions = _transactionService.GetAll(userId, accountId);
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public ActionResult<Transaction> GetById(int id)
    {
        var userId = GetUserIdFromClaims();
        var transaction = _transactionService.GetById(id);

        if (transaction == null) return NotFound();

        // Sprawdź, czy użytkownik jest właścicielem transakcji
        if (transaction.OwnerId != userId)
        {
            return Forbid("You do not have access to this transaction.");
        }

        return Ok(transaction);
    }

    [HttpPost]
    public ActionResult<Transaction> Create([FromBody] CreateTransactionDto transactionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerId = GetUserIdFromClaims();
        var (createdTransaction, _, _) = _transactionService.Create(transactionDto, ownerId, out string? errorMessage);

        if (createdTransaction == null)
        {
            return BadRequest(new { message = errorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateTransactionDto transactionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserIdFromClaims();
        var (updatedTransaction, _, _) = _transactionService.Update(id, transactionDto, userId, out string? errorMessage);

        if (updatedTransaction == null)
        {
            // Potencjalnie Forbid lub NotFound w zależności od błędu
            return BadRequest(new { message = errorMessage });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = GetUserIdFromClaims();
        decimal oldAmount;
        int oldAccountId;

        var result = _transactionService.Delete(id, userId, out oldAmount, out oldAccountId);
        if (!result)
        {
            var transaction = _transactionService.GetById(id);
            if (transaction == null) return NotFound();
            return Forbid(); // Użytkownik nie ma uprawnień do usunięcia
        }

        return NoContent();
    }
}
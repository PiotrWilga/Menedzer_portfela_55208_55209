using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PersonalFinanceManager.WebApi.Dtos;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/accounts/{accountId}/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;

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
    public ActionResult<IEnumerable<TransactionDto>> GetAll(int accountId)
    {
        var userId = GetUserIdFromClaims();

        if (!_accountService.HasAccountAccess(accountId, userId))
        {
            return Forbid("You do not have access to the specified account.");
        }

        var transactions = _transactionService.GetAll(accountId, userId);
        return Ok(transactions);
    }

    [HttpGet("{transactionId}")]
    public ActionResult<TransactionDto> GetById(int accountId, int transactionId)
    {
        var userId = GetUserIdFromClaims();

        if (!_accountService.HasAccountAccess(accountId, userId))
        {
            return Forbid("You do not have access to the specified account.");
        }

        var transactionDto = _transactionService.GetById(transactionId, accountId);

        if (transactionDto == null) return NotFound();

        if (transactionDto.OwnerId != userId)
        {
            return Forbid("You do not have access to this transaction.");
        }
        if (transactionDto.AccountId != accountId)
        {
            return NotFound("Transaction does not belong to the specified account.");
        }

        return Ok(transactionDto);
    }

    [HttpPost]
    public ActionResult<TransactionDto> Create(int accountId, [FromBody] CreateTransactionDto transactionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ownerId = GetUserIdFromClaims();

        var (createdTransactionModel, _, _) = _transactionService.Create(accountId, transactionDto, ownerId, out string? errorMessage);

        if (createdTransactionModel == null)
        {
            return BadRequest(new { message = errorMessage });
        }

        var returnedDto = _transactionService.GetById(createdTransactionModel.Id, createdTransactionModel.AccountId);
        if (returnedDto == null)
        {
            return StatusCode(500, "Failed to retrieve created transaction data.");
        }

        return CreatedAtAction(nameof(GetById), new { accountId = returnedDto.AccountId, transactionId = returnedDto.Id }, returnedDto);
    }

    [HttpPut("{transactionId}")]
    public IActionResult Update(int accountId, int transactionId, [FromBody] UpdateTransactionDto transactionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserIdFromClaims();
        var (updatedTransactionModel, _, _) = _transactionService.Update(transactionId, accountId, transactionDto, userId, out string? errorMessage);

        if (updatedTransactionModel == null)
        {
            return BadRequest(new { message = errorMessage });
        }

        return NoContent();
    }

    [HttpDelete("{transactionId}")]
    public IActionResult Delete(int accountId, int transactionId)
    {
        var userId = GetUserIdFromClaims();
        decimal oldAmount;
        int oldAccountId;

        var result = _transactionService.Delete(transactionId, accountId, userId, out oldAmount, out oldAccountId);
        if (!result)
        {
            var transaction = _transactionService.GetById(transactionId, accountId);
            if (transaction == null) return NotFound();
            return Forbid();
        }

        return NoContent();
    }
}
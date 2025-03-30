using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.WebApi.Models;
using PersonalFinanceManager.WebApi.Services;
using System.Collections.Generic;

namespace PersonalFinanceManager.WebApi.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Account>> GetAll()
    {
        return Ok(_accountService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Account> GetById(int id)
    {
        var account = _accountService.GetById(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpPost]
    public ActionResult<Account> Create([FromBody] Account account)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdAccount = _accountService.Create(account);
        return CreatedAtAction(nameof(GetById), new { id = createdAccount.Id }, createdAccount);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Account updatedAccount)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = _accountService.Update(id, updatedAccount);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _accountService.Delete(id);
        if (!result) return NotFound();
        return NoContent();
    }
}

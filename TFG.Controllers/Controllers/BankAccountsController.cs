using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Services;
using TFG.Services.Exceptions;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("[controller]")]
public class BankAccountsController(BankAccountService bankAccountService) : ControllerBase
{
    //GET
    [HttpGet]
    public async Task<ActionResult<Pagination<BankAccountResponseDto>>> GetBankAccounts([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Iban", [FromQuery] bool descending = false,
        [FromQuery] string? search = null, [FromQuery] string? filter = null, [FromQuery] bool? isDeleted = null)
    {
        return await bankAccountService.GetBankAccounts(pageNumber, pageSize, orderBy, descending, search, filter,
            isDeleted);
    }
    
    [HttpGet("{iban}")]
    public async Task<ActionResult<BankAccountResponseDto>> GetBankAccount(string iban)
    {
        return await bankAccountService.GetBankAccount(iban);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<BankAccountResponseDto>>> GetBankAccountsByUserId(Guid userId)
    {
        return await bankAccountService.GetBankAccountsByUserId(userId);
    }
    
    [Authorize(Policy = "User")]
    [HttpGet("my-self/user")]
    public async Task<ActionResult<List<BankAccountResponseDto>>> GetBankAccountsByMySelf()
    {
        return await bankAccountService.GetBankAccountsByUserId(GetUserId());
    }
    
    [Authorize(Policy = "User")]
    [HttpGet("my-self/totalbalance")]
    public async Task<ActionResult<decimal>> GetTotalBalanceByMySelf()
    {
        return await bankAccountService.GetTotalBalanceByUserId(GetUserId());
    }

    //CREATE
    [HttpPost]
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    //UPDATE
    [HttpPut("{iban}")]
    public async Task<ActionResult<BankAccountResponseDto>> UpdateBankAccount(string iban,
        [FromBody] BankAccountUpdateDto bankAccount)
    {
        return await bankAccountService.UpdateBankAccount(iban, bankAccount);
    }

    //DELETE
    [HttpDelete("{iban}")]
    public async Task<ActionResult> DeleteBankAccount(string iban)
    {
        await bankAccountService.DeleteBankAccount(iban);
        return NoContent();
    }

    //ACTIVE
    [HttpPut("{iban}/active")]
    public async Task<ActionResult> ActiveBankAccount(string iban)
    {
        await bankAccountService.ActivateBankAccount(iban);
        return Ok();
    }

    [HttpPost("{iban}/active-bizum/{userId}")]
    public async Task<ActionResult> ActiveBizum(string iban, Guid userId)
    {
        await bankAccountService.ActiveBizum(iban, userId);
        return Ok();
    }
    
    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new HttpException(401, "Unauthorized"));
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Services;
using TFG.Services.Pagination;

namespace TFG.Controllers.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("[controller]")]
public class BankAccountsController(BankAccountService bankAccountService) : ControllerBase
{
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

    [HttpGet("{bankAccountIban}/transactions")]
    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(string bankAccountIban)
    {
        return await bankAccountService.GetTransactionsForAccount(bankAccountIban);
    }

    [HttpGet("{bankAccountIban}/expenses")]
    public async Task<List<TransactionResponseDto>> GetExpensesForAccount(string bankAccountIban)
    {
        return await bankAccountService.GetExpensesForAccount(bankAccountIban);
    }

    [HttpGet("{bankAccountIban}/incomes")]
    public async Task<List<TransactionResponseDto>> GetIncomesForAccount(string bankAccountIban)
    {
        return await bankAccountService.GetIncomesForAccount(bankAccountIban);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<BankAccountResponseDto>>> GetBankAccountsByUserId(Guid userId)
    {
        return await bankAccountService.GetBankAccountsByUserId(userId);
    }

    [HttpPost]
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    [HttpPut("{iban}")]
    public async Task<ActionResult<BankAccountResponseDto>> UpdateBankAccount(string iban,
        [FromBody] BankAccountUpdateDto bankAccount)
    {
        return await bankAccountService.UpdateBankAccount(iban, bankAccount);
    }

    [HttpDelete("{iban}")]
    public async Task<ActionResult> DeleteBankAccount(string iban)
    {
        await bankAccountService.DeleteBankAccount(iban);
        return NoContent();
    }

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
}
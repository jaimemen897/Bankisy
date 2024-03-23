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
    [HttpGet()]
    public async Task<ActionResult<Pagination<BankAccountResponseDto>>> GetBankAccounts([FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Iban", [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string? filter = null)
    {
        return await bankAccountService.GetBankAccounts(pageNumber, pageSize, orderBy, descending, search, filter);
    }

    [HttpGet("{iban}")]
    public async Task<ActionResult<BankAccountResponseDto>> GetBankAccount(string iban)
    {
        return await bankAccountService.GetBankAccount(iban);
    }
    
    [HttpGet("user/{userId}")]
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUser(Guid userId)
    {
        return await bankAccountService.GetBankAccountsByUserId(userId);
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
    
    [HttpPost()]
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    [HttpPut("{iban}")]
    public async Task<ActionResult<BankAccountResponseDto>> UpdateBankAccount(string iban, BankAccountUpdateDto bankAccount)
    {
       return await bankAccountService.UpdateBankAccount(iban, bankAccount);
    }

    [HttpDelete("{iban}")]
    public async Task DeleteBankAccount(string iban)
    {
        await bankAccountService.DeleteBankAccount(iban);
    }
    
    
}
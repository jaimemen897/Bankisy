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
        [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Id", [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string? filter = null)
    {
        return await bankAccountService.GetBankAccounts(pageNumber, pageSize, orderBy, descending, search, filter);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccountResponseDto>> GetBankAccount(Guid id)
    {
        return await bankAccountService.GetBankAccount(id);
    }
    
    [HttpGet("user/{userId}")]
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUser(Guid userId)
    {
        return await bankAccountService.GetBankAccountsByUserId(userId);
    }
    
    [HttpGet("{bankAccountId}/transactions")]
    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(Guid bankAccountId)
    {
        return await bankAccountService.GetTransactionsForAccount(bankAccountId);
    }
    
    [HttpGet("{bankAccountId}/expenses")]
    public async Task<List<TransactionResponseDto>> GetExpensesForAccount(Guid bankAccountId)
    {
        return await bankAccountService.GetExpensesForAccount(bankAccountId);
    }
    
    [HttpGet("{bankAccountId}/incomes")]
    public async Task<List<TransactionResponseDto>> GetIncomesForAccount(Guid bankAccountId)
    {
        return await bankAccountService.GetIncomesForAccount(bankAccountId);
    }
    
    [HttpPost()]
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BankAccountResponseDto>> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
       return await bankAccountService.UpdateBankAccount(id, bankAccount);
    }

    [HttpDelete("{id}")]
    public async Task DeleteBankAccount(Guid id)
    {
        await bankAccountService.DeleteBankAccount(id);
    }
    
    
}
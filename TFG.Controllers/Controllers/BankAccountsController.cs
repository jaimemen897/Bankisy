using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class BankAccountsController(BankAccountService bankAccountService) : ControllerBase
{
    [HttpGet()]
    public async Task<List<BankAccountResponseDto>> GetBankAccounts()
    {
        return await bankAccountService.GetBankAccounts();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccountResponseDto>> GetBankAccount(Guid id)
    {
        return await bankAccountService.GetBankAccountAsync(id);
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
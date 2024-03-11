using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
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
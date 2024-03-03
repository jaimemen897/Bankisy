using Microsoft.AspNetCore.Mvc;
using TFG.Context.DTOs.bankAccount;
using TFG.Services;

namespace TFG.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class BankAccountsController : ControllerBase
{
    private readonly BankAccountService _bankAccountService;

    public BankAccountsController(BankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    [HttpGet()]
    public async Task<ActionResult<List<BankAccountResponseDto>>> GetBankAccounts()
    {
        return await _bankAccountService.GetBankAccounts();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccountResponseDto>> GetBankAccount(Guid id)
    {
        var bankAccount = await _bankAccountService.GetBankAccountAsync(id);
        return bankAccount == null ? NotFound() : bankAccount;
    }

    [HttpPost()]
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await _bankAccountService.CreateBankAccount(bankAccount);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BankAccountResponseDto>> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
        var bankAccountUpdated = await _bankAccountService.UpdateBankAccount(id, bankAccount);
        return bankAccountUpdated == null ? NotFound() : bankAccountUpdated;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBankAccount(Guid id)
    {
        return await _bankAccountService.DeleteBankAccount(id) ? Ok() : NotFound();
    }
}
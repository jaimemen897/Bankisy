using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.Models;
using TFG.Services.mappers;

namespace TFG.Services;

public class BankAccountService
{
    private readonly BankContext _bankContext;
    private readonly Mapper _mapper;

    public BankAccountService(BankContext bankContext)
    {
        _bankContext = bankContext;
        _mapper = MapperConfig.InitializeAutomapper();
    }
    

    public async Task<List<BankAccountResponseDto>> GetBankAccounts()
    {
        var bankAccountList = await _bankContext.BankAccounts.ToListAsync();
        return bankAccountList.Select(bankAccount => _mapper.Map<BankAccountResponseDto>(bankAccount)).ToList();
    }

    public async Task<BankAccountResponseDto?> GetBankAccountAsync(Guid id)
    {
        var bankAccount = await _bankContext.BankAccounts.FindAsync(id);
        return bankAccount == null ? null : _mapper.Map<BankAccountResponseDto>(bankAccount);
    }

    private static ActionResult IsValid(BankAccountCreateDto bankAccountCreateDto)
    {
        if (!Enum.TryParse(typeof(AccountType), bankAccountCreateDto.AccountType, out _))
        {
            return new BadRequestObjectResult(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {
                    "AccountType",
                    new[]
                    {
                        "Invalid account type. Valid values are: " +
                        string.Join(", ", Enum.GetNames(typeof(AccountType)))
                    }
                }
            }));
        }

        return new OkResult();
    }

    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccountCreateDto)
    {
        IsValid(bankAccountCreateDto);

        var user = await _bankContext.Users.FindAsync(bankAccountCreateDto.UserId);

        if (user == null)
        {
            return new NotFoundResult();
        }

        var bankAccount = _mapper.Map<BankAccount>(bankAccountCreateDto);
        user.BankAccounts.Add(bankAccount);

        _bankContext.BankAccounts.Add(bankAccount);
        await _bankContext.SaveChangesAsync();

        var bankAccountResponseDto = _mapper.Map<BankAccountResponseDto>(bankAccount);
        return bankAccountResponseDto;
    }

    public async Task<BankAccountResponseDto?> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
        var bankAccountToUpdate = await _bankContext.BankAccounts.FindAsync(id);
        if (bankAccountToUpdate == null)
        {
            return null;
        }
        
        bankAccountToUpdate = _mapper.Map(bankAccount, bankAccountToUpdate);
        await _bankContext.SaveChangesAsync();

        return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
    }

    public async Task<bool> DeleteBankAccount(Guid id)
    {
        var bankAccount = await _bankContext.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return false;
        }

        _bankContext.BankAccounts.Remove(bankAccount);
        await _bankContext.SaveChangesAsync();
        return true;
    }
}
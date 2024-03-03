using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.Models;
using TFG.Services.mappers;

namespace TFG.Services;

public class BankAccountService
{
    private readonly BankContext _bankContext;

    public BankAccountService(BankContext bankContext)
    {
        _bankContext = bankContext;
    }

    public async Task<List<BankAccountResponseDto>> GetBankAccounts()
    {
        var bankAccountList = await _bankContext.BankAccounts.ToListAsync();
        return bankAccountList.Select(BankAccountMapper.MapToResponseDto).ToList();
    }

    public async Task<BankAccountResponseDto?> GetBankAccountAsync(Guid id)
    {
        var bankAccount = await _bankContext.BankAccounts.FindAsync(id);
        return bankAccount == null ? null : BankAccountMapper.MapToResponseDto(bankAccount);
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
        var validationResult = IsValid(bankAccountCreateDto);

        var user = await _bankContext.Users.FindAsync(bankAccountCreateDto.UserId);

        if (user == null)
        {
            return new NotFoundResult();
        }

        var bankAccount = BankAccountMapper.MapToEntity(bankAccountCreateDto);
        user.BankAccounts.Add(bankAccount);

        _bankContext.BankAccounts.Add(bankAccount);
        await _bankContext.SaveChangesAsync();

        var bankAccountResponseDto = BankAccountMapper.MapToResponseDto(bankAccount);
        return bankAccountResponseDto;
    }

    public async Task<BankAccountResponseDto?> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
        var bankAccountToUpdate = await _bankContext.BankAccounts.FindAsync(id);
        if (bankAccountToUpdate == null)
        {
            return null;
        }

        bankAccountToUpdate = BankAccountMapper.MapToEntity(bankAccountToUpdate, bankAccount);
        await _bankContext.SaveChangesAsync();

        return BankAccountMapper.MapToResponseDto(bankAccountToUpdate);
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
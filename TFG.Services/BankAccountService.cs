using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class BankAccountService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<List<BankAccountResponseDto>> GetBankAccounts()
    {
        var bankAccountList = await bankContext.BankAccounts.ToListAsync();
        return bankAccountList.Where(account => !account.IsDeleted).Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();
    }

    public async Task<BankAccountResponseDto> GetBankAccountAsync(Guid id)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(id);
        return bankAccount == null ? throw new HttpException(404, "BankAccount not found"): _mapper.Map<BankAccountResponseDto>(bankAccount);
    }

    public async Task<BankAccountResponseDto> CreateBankAccount(BankAccountCreateDto bankAccountCreateDto)
    {
        IsValid(bankAccountCreateDto);

        var user = await bankContext.Users.FindAsync(bankAccountCreateDto.UserId);

        if (user == null)
        {
            throw new HttpException(404, "User not found");
        }

        var bankAccount = _mapper.Map<BankAccount>(bankAccountCreateDto);
        user.BankAccounts.Add(bankAccount);

        bankContext.BankAccounts.Add(bankAccount);
        await bankContext.SaveChangesAsync();

        var bankAccountResponseDto = _mapper.Map<BankAccountResponseDto>(bankAccount);
        return bankAccountResponseDto;
    }

    public async Task<BankAccountResponseDto> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
        var bankAccountToUpdate = await bankContext.BankAccounts.FindAsync(id);
        if (bankAccountToUpdate == null)
        {
            throw new HttpException(404, "Bank account not found");
        }

        if (bankAccount.UserId != null)
        {
            var user = await bankContext.Users.FindAsync(bankAccount.UserId);
            if (user == null)
            {
                throw new HttpException(404, "User not found");
            }
            bankAccountToUpdate.UserId = bankAccount.UserId.Value;
        }

        bankAccountToUpdate = _mapper.Map(bankAccount, bankAccountToUpdate);
        await bankContext.SaveChangesAsync();

        return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
    }

    public async Task DeleteBankAccount(Guid id)
    {
        var bankAccount = await bankContext.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            throw new HttpException(404, "Bank account not found");
        }

        bankAccount.IsDeleted = true;
        await bankContext.SaveChangesAsync();
    }

    private static void IsValid(BankAccountCreateDto bankAccountCreateDto)
    {
        if (!Enum.TryParse(typeof(AccountType), bankAccountCreateDto.AccountType, out _))
        {
            throw new HttpException(400,
                "Invalid account type. Valid values are: " + string.Join(", ", Enum.GetNames(typeof(AccountType))));
        }
    }
}
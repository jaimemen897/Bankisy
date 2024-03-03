using System.ComponentModel.DataAnnotations;
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
        return bankAccount == null ? null: BankAccountMapper.MapToResponseDto(bankAccount);
    }
    
    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccountCreateDto)
    {
        
        
        
        var userExists = await _bankContext.Users.FindAsync(bankAccountCreateDto.UserId) != null;

        if (!userExists)
        {
            return new NotFoundResult();
        }

        var bankAccount = BankAccountMapper.MapToEntity(bankAccountCreateDto);

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
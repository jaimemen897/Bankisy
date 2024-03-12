using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class BankAccountService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<List<BankAccountResponseDto>> GetBankAccounts()
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        return bankAccountList.Where(account => !account.IsDeleted)
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();
    }

    public async Task<BankAccountResponseDto> GetBankAccountAsync(Guid id)
    {
        var bankAccount =
            await bankContext.BankAccounts.Include(ba => ba.UsersId).FirstOrDefaultAsync(ba => ba.Id == id);
        return bankAccount == null
            ? throw new HttpException(404, "BankAccount not found")
            : _mapper.Map<BankAccountResponseDto>(bankAccount);
    }

    public async Task<BankAccountResponseDto> CreateBankAccount(BankAccountCreateDto bankAccountCreateDto)
    {
        IsValid(bankAccountCreateDto);
        var users = await bankContext.Users.Where(u => bankAccountCreateDto.UsersId.Contains(u.Id)).ToListAsync();

        if (users.Count != bankAccountCreateDto.UsersId.Count)
        {
            throw new HttpException(404, "Users not found");
        }

        var bankAccount = _mapper.Map<BankAccount>(bankAccountCreateDto);
        bankAccount.UsersId = users;
        foreach (var user in users)
        {
            user.BankAccounts.Add(bankAccount);
        }

        bankContext.BankAccounts.Add(bankAccount);
        await bankContext.SaveChangesAsync();

        var bankAccountResponseDto = _mapper.Map<BankAccountResponseDto>(bankAccount);
        return bankAccountResponseDto;
    }

    public async Task<BankAccountResponseDto> UpdateBankAccount(Guid id, BankAccountUpdateDto bankAccount)
    {
        var bankAccountToUpdate =
            await bankContext.BankAccounts.Include(ba => ba.UsersId).FirstOrDefaultAsync(ba => ba.Id == id) ??
            throw new HttpException(404, "Bank account not found");

        if (bankAccount.UsersId != null)
        {
            var users = await bankContext.Users.Where(u => bankAccount.UsersId.Contains(u.Id)).ToListAsync();
            if (users.Count != bankAccount.UsersId.Count)
            {
                throw new HttpException(404, "Users not found");
            }

            foreach (var user in users)
            {
                bankAccountToUpdate.UsersId.Add(user);
                user.BankAccounts.Add(bankAccountToUpdate);
            }

            await bankContext.SaveChangesAsync();

            return _mapper.Map<BankAccountResponseDto>(bankAccountToUpdate);
        }

        _mapper.Map(bankAccount, bankAccountToUpdate);

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
    
    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId(Guid userId)
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        return bankAccountList.Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();
    }
    
    public async Task<List<TransactionResponseDto>> GetTransactionsForAccount(Guid bankAccountId)
    {
        var transactions = await bankContext.Transactions
            .Where(t => t.IdAccountOrigin == bankAccountId || t.IdAccountDestination == bankAccountId)
            .ToListAsync() ?? throw new HttpException(404, "Transactions not found");
        if (transactions.Count == 0)
        {
            throw new HttpException(404, "Transactions not found");
        }
        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }
    
    public async Task<List<TransactionResponseDto>> GetExpensesForAccount(Guid bankAccountId)
    {
        var transactions = await bankContext.Transactions
            .Where(t => t.IdAccountOrigin == bankAccountId)
            .ToListAsync();
        if (transactions.Count == 0)
        {
            throw new HttpException(404, "Transactions not found");
        }
        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }
}
using System.Security.Claims;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class IndexService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    
    private async Task<bool> GetCurrentUserIsAdmin(Guid userId)
    {
        var user = await bankContext.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new HttpException(404, "User not found");
        return user.Role.Equals(Roles.Admin);
    }

    public async Task<List<BankAccountResponseDto>> GetBankAccountsByUserId(Guid userId)
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        return bankAccounts ?? throw new HttpException(404, "BankAccounts not found");
    }

    public async Task<decimal> GetTotalBalanceByUserId(Guid userId)
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();
        return bankAccounts.Sum(ba => ba.Balance);
    }

    public async Task<List<TransactionResponseDto>> GetTransactionsByUserId(Guid userId)
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        var transactions = new List<Transaction>();

        foreach (var bankAccount in bankAccounts)
        {
            transactions.AddRange(await bankContext.Transactions
                .Where(t => bankAccount.Iban == t.IbanAccountOrigin || bankAccount.Iban == t.IbanAccountDestination)
                .ToListAsync());
        }

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetExpensesByUserId(Guid userId)
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        var transactions = new List<Transaction>();

        foreach (var bankAccount in bankAccounts)
        {
            transactions.AddRange(await bankContext.Transactions
                .Where(t => bankAccount.Iban == t.IbanAccountOrigin)
                .ToListAsync());
        }

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<List<TransactionResponseDto>> GetIncomesByUserId(Guid userId)
    {
        var bankAccountList = await bankContext.BankAccounts.Include(ba => ba.UsersId).ToListAsync();
        var bankAccounts = bankAccountList
            .Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => _mapper.Map<BankAccountResponseDto>(account)).ToList();

        var transactions = new List<Transaction>();

        foreach (var bankAccount in bankAccounts)
        {
            transactions.AddRange(await bankContext.Transactions
                .Where(t => bankAccount.Iban == t.IbanAccountDestination)
                .ToListAsync());
        }

        return transactions.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }
}
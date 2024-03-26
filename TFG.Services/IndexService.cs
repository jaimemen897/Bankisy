using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Extensions;
using TFG.Services.mappers;
using TFG.Services.Pagination;

namespace TFG.Services;

public class IndexService(
    BankContext bankContext,
    BankAccountService bankAccountService,
    TransactionService transactionService)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    
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

    /*public async Task<List<TransactionResponseDto>> GetTransactionsByUserId(Guid userId)
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
    }*/

    public async Task<Pagination<TransactionResponseDto>> GetTransactionsByUserId(Guid userId, int pageNumber, int pageSize,
        string orderBy, bool descending, string? search = null)
    {
        pageNumber = pageNumber > 0 ? pageNumber : 1;
        pageSize = pageSize > 0 ? pageSize : 10;

        if (!typeof(TransactionResponseDto).GetProperties()
                .Any(p => string.Equals(p.Name, orderBy, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new HttpException(400, "Invalid orderBy parameter");
        }

        var bankAccountIbans = await bankContext.BankAccounts
            .Where(account => !account.IsDeleted && account.UsersId.Any(u => u.Id == userId))
            .Select(account => account.Iban)
            .ToListAsync();

        var transactionQuery = bankContext.Transactions
            .Where(t => bankAccountIbans.Contains(t.IbanAccountOrigin) || bankAccountIbans.Contains(t.IbanAccountDestination));

        if (!string.IsNullOrEmpty(search))
        {
            transactionQuery = transactionQuery.Where(t => t.IbanAccountOrigin.ToLower().Contains(search.ToLower()) ||
                                                           t.IbanAccountDestination.ToLower().Contains(search.ToLower()));
        }

        var paginatedTransactions = await transactionQuery.ToPagination(pageNumber, pageSize, orderBy, descending,
            transaction => _mapper.Map<TransactionResponseDto>(transaction));

        return paginatedTransactions;
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

    public async Task<ActionResult<BankAccountResponseDto>> CreateBankAccount(BankAccountCreateDto bankAccount)
    {
        return await bankAccountService.CreateBankAccount(bankAccount);
    }

    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transaction)
    {
        return await transactionService.CreateTransaction(transaction);
    }
}
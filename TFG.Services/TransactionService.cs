using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.mappers;

namespace TFG.Services;

public class TransactionService(BankContext bankContext)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();

    public async Task<List<TransactionResponseDto>> GetTransactions()
    {
        var transactionList = await bankContext.Transactions.ToListAsync();
        return transactionList.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<TransactionResponseDto> GetTransaction(int id)
    {
        var transaction = await bankContext.Transactions.FindAsync(id);
        return transaction == null ? throw new HttpException(404, "Transaction not found"): _mapper.Map<TransactionResponseDto>(transaction);
    }

    public async Task<TransactionResponseDto> CreateTransaction(TransactionCreateDto transactionCreateDto)
    {
        var account = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountOrigin);
        var accountDestination = await bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountDestination);
        
        if (account == null || accountDestination == null)
        {
            throw new HttpException(404, "Account not found");
        }
        
        var transaction = _mapper.Map<Transaction>(transactionCreateDto);
        
        account.Transactions.Add(transaction);
        
        bankContext.Transactions.Add(transaction);
        
        account.Balance -= transaction.Amount;
        accountDestination.Balance += transaction.Amount;
        
        await bankContext.SaveChangesAsync();

        var transactionResponseDto = _mapper.Map<TransactionResponseDto>(transaction);
        return transactionResponseDto;
    }

    public async Task DeleteTransaction(int id)
    {
        var transaction = await bankContext.Transactions.FindAsync(id);
        if (transaction == null)
        {
            throw new HttpException(404, "Transaction not found");
        }

        bankContext.Transactions.Remove(transaction);
        await bankContext.SaveChangesAsync();
    }
}
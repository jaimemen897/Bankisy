using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFG.Context.Context;
using TFG.Context.DTOs.transactions;
using TFG.Context.Models;
using TFG.Services.mappers;

namespace TFG.Services;

public class TransactionService
{
    private readonly BankContext _bankContext;
    private readonly Mapper _mapper;

    public TransactionService(BankContext bankContext)
    {
        _bankContext = bankContext;
        _mapper = MapperConfig.InitializeAutomapper();
    }

    public async Task<List<TransactionResponseDto>> GetTransactions()
    {
        var transactionList = await _bankContext.Transactions.ToListAsync();
        return transactionList.Select(transaction => _mapper.Map<TransactionResponseDto>(transaction)).ToList();
    }

    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        var transaction = await _bankContext.Transactions.FindAsync(id);
        return transaction == null ? new NotFoundResult() : _mapper.Map<TransactionResponseDto>(transaction);
    }

    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(TransactionCreateDto transactionCreateDto)
    {
        var account = await _bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountOrigin);
        var accountDestination = await _bankContext.BankAccounts.FindAsync(transactionCreateDto.IdAccountDestination);
        if (account == null || accountDestination == null)
        {
            return new NotFoundResult();
        }
        
        var transaction = _mapper.Map<Transaction>(transactionCreateDto);
        
        account.Transactions.Add(transaction);
        
        _bankContext.Transactions.Add(transaction);
        
        account.Balance -= transaction.Amount;
        accountDestination.Balance += transaction.Amount;
        
        await _bankContext.SaveChangesAsync();

        var transactionResponseDto = _mapper.Map<TransactionResponseDto>(transaction);
        return transactionResponseDto;
    }

    public async Task<ActionResult<TransactionResponseDto>> UpdateTransaction(int id, TransactionUpdateDto transactionDto)
    {
        var transactionToUpdate = await _bankContext.Transactions.FindAsync(id);
        if (transactionToUpdate == null)
        {
            return new NotFoundResult();
        }

        transactionToUpdate = _mapper.Map(transactionDto, transactionToUpdate);
        await _bankContext.SaveChangesAsync();

        return _mapper.Map<TransactionResponseDto>(transactionToUpdate);
    }

    public async Task<ActionResult<bool>> DeleteTransaction(int id)
    {
        var transaction = await _bankContext.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return new NotFoundResult();
        }

        _bankContext.Transactions.Remove(transaction);
        await _bankContext.SaveChangesAsync();
        return true;
    }
}
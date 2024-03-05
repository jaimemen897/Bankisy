using TFG.Context.DTOs.transactions;
using TFG.Context.Models;

namespace TFG.Services.mappers;

public class TransactionMapper
{
    public static Transaction MapToEntity(TransactionCreateDto transactionDto)
    {
        return new Transaction
        {
            Concept = transactionDto.Concept,
            Amount = transactionDto.Amount,
            IdAccountOrigin = transactionDto.IdAccountOrigin,
            IdAccountDestination = transactionDto.IdAccountDestination,
            Date = DateTime.Now.ToUniversalTime(),
        };
    }

    public static Transaction MapToEntity(Transaction transaction, TransactionUpdateDto transactionDto)
    {
        transaction.Concept = transactionDto.Concept ?? transaction.Concept;
        transaction.Amount = transactionDto.Amount ?? transaction.Amount;
        transaction.IdAccountOrigin = transactionDto.IdAccountOrigin ?? transaction.IdAccountOrigin;
        transaction.IdAccountDestination = transactionDto.IdAccountDestination ?? transaction.IdAccountDestination;
        transaction.Date = transactionDto.Date ?? transaction.Date;
        return transaction;
    }

    public static TransactionResponseDto MapToResponseDto(Transaction transaction)
    {
        return new TransactionResponseDto
        {
            Id = transaction.Id,
            Concept = transaction.Concept,
            Amount = transaction.Amount,
            IdAccountOrigin = transaction.IdAccountOrigin,
            IdAccountDestination = transaction.IdAccountDestination,
            Date = transaction.Date,
        };
    }
}
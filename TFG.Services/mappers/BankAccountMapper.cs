using TFG.Context.DTOs.bankAccount;
using TFG.Context.Models;

namespace TFG.Services.mappers;

public class BankAccountMapper
{
    public static BankAccount MapToEntity(BankAccountCreateDto bankAccountDto)
    {
        var accountType = Enum.Parse<AccountType>(bankAccountDto.AccountType);
        
        return new BankAccount
        {
            Id = new Guid(),
            Balance = bankAccountDto.Balance,
            AccountType = accountType,
            UserId = bankAccountDto.UserId,
        };
    }
    
    public static BankAccount MapToEntity(BankAccount bankAccount, BankAccountUpdateDto bankAccountDto)
    {
        bankAccount.Balance = bankAccountDto.Balance ?? bankAccount.Balance;
        bankAccount.AccountType = bankAccountDto.AccountType ?? bankAccount.AccountType;
        return bankAccount;
    }
    
    public static BankAccountResponseDto MapToResponseDto(BankAccount bankAccount)
    {
        return new BankAccountResponseDto
        {
            Id = bankAccount.Id,
            UserId = bankAccount.UserId,
            Balance = bankAccount.Balance,
            AccountType = bankAccount.AccountType,
        };
    }
}
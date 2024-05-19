using AutoMapper;
using Microsoft.AspNetCore.Http;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Exceptions;
using TFG.Services.Mappers;

namespace TFG.Services;

public class IndexService(
    BankAccountService bankAccountService,
    TransactionService transactionService,
    SessionService sessionService,
    UsersService usersService)
{
    private readonly Mapper _mapper = MapperConfig.InitializeAutomapper();
    private readonly UserResponseDto user = sessionService.GetMyself().Result;

    public UserResponseDto GetMyself()
    {
        return sessionService.GetMyself().Result;
    }

    //PROFILE
    public async Task<string> UpdateProfile(UserUpdateDto userUpdateDto)
    {
        var userUpdated = await usersService.UpdateUser(user.Id, userUpdateDto);
        var userMapped = _mapper.Map<User>(userUpdated);
        return sessionService.GetToken(userMapped);
    }

    public async Task<UserResponseDto> UploadAvatar(IFormFile file, string host)
    {
        return await usersService.UploadAvatar(user.Id, file, host);
    }

    public async Task DeleteAvatar()
    {
        await usersService.DeleteAvatar(user.Id);
    }

    //PAYMENT INTENT
    public async Task AddPaymentIntent(decimal ammount, Guid userId, string iban)
    {
        var userAsync = await usersService.GetUserAsync(userId);

        var bankAccount = await bankAccountService.GetBankAccount(iban);
        if (bankAccount.UsersId.All(id => id != userAsync.Id))
            throw new HttpException(403, "You are not the owner of the account");

        var incomeCreateDto = new IncomeCreateDto
        {
            Amount = ammount,
            IbanAccountDestination = bankAccount.Iban
        };
        await transactionService.AddPaymentIntent(incomeCreateDto);
    }
}
using AutoMapper;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;

namespace TFG.Services.mappers;

public class MapperConfig
{
    public static Mapper InitializeAutomapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            /*BANK ACOUNTS*/
            cfg.CreateMap<BankAccountCreateDto, BankAccount>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => new Guid()))
                .ForMember(dest => dest.AccountType,
                    act => act.MapFrom(src => Enum.Parse<AccountType>(src.AccountType)))
                .ForMember(dest => dest.Transactions, act => act.MapFrom(src => new List<Transaction>()));
            cfg.CreateMap<BankAccountUpdateDto, BankAccount>()
                .ForMember(dest => dest.Balance, act => act.MapFrom(src => src.Balance ?? src.Balance))
                .ForMember(dest => dest.AccountType, act => act.MapFrom(src => src.AccountType ?? src.AccountType))
                .ForMember(dest => dest.UserId, act => act.MapFrom(src => src.UserId ?? src.UserId));
            cfg.CreateMap<BankAccount, BankAccountResponseDto>();

            /*USERS*/
            cfg.CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => new Guid()))
                .ForMember(dest => dest.CreatedAt, act => act.MapFrom(src => DateTime.Now.ToUniversalTime()))
                .ForMember(dest => dest.UpdatedAt, act => act.MapFrom(src => DateTime.Now.ToUniversalTime()))
                .ForMember(dest => dest.BankAccounts, act => act.MapFrom(src => new List<BankAccount>()));
            cfg.CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name ?? src.Name))
                .ForMember(dest => dest.Email, act => act.MapFrom(src => src.Email ?? src.Email))
                .ForMember(dest => dest.Avatar, act => act.MapFrom(src => src.Avatar ?? src.Avatar))
                .ForMember(dest => dest.UpdatedAt, act => act.MapFrom(src => DateTime.Now.ToUniversalTime()));
            cfg.CreateMap<User, UserResponseDto>();
            
            /*TRANSACTIONS*/
            cfg.CreateMap<TransactionCreateDto, Transaction>()
                .ForMember(dest => dest.Date, act => act.MapFrom(src => DateTime.Now.ToUniversalTime()));
            cfg.CreateMap<TransactionUpdateDto, Transaction>();
            cfg.CreateMap<Transaction, TransactionResponseDto>();

        });
        
        return new Mapper(config);
    }
}
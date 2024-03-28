using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.cards;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;
using TFG.Services.Security;

namespace TFG.Services.mappers;

public abstract class MapperConfig
{
    public static Mapper InitializeAutomapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            /*BANK ACOUNTS*/
            cfg.CreateMap<BankAccountCreateDto, BankAccount>()
                .ForMember(dest => dest.AccountType, act => act.MapFrom(src => src.AccountType))
                .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => false))
                .ForMember(dest => dest.UsersId, act => act.Ignore());

            cfg.CreateMap<BankAccountUpdateDto, BankAccount>()
                .ForMember(dest => dest.AccountType, opt => opt.Ignore())
                .ForMember(dest => dest.UsersId, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.AccountType = src.AccountType != null ? (AccountType)Enum.Parse(typeof(AccountType), src.AccountType) : dest.AccountType;
                    dest.UsersId = src.UsersId != null ? src.UsersId.Select(id => new User { Id = id }).ToList() : dest.UsersId;
                });

            cfg.CreateMap<BankAccount, BankAccountResponseDto>()
                .ForMember(dest => dest.AccountType, act => act.MapFrom(src => src.AccountType.ToString()))
                .ForMember(dest => dest.UsersId, act => act.MapFrom(src => src.UsersId.Select(u => u.Id).ToList()))
                .ForMember(dest => dest.UsersName, act => act.MapFrom(src => src.UsersId.Select(u => u.Name).ToList()));

            /*USERS*/
            cfg.CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Password, act => act.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
                .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => false))
                .ForMember(dest => dest.Avatar, act => act.MapFrom(src => src.Avatar ?? User.ImageDefault))
                .ForMember(dest => dest.Role, act => act.MapFrom(src => Roles.User))
                .ForMember(dest => dest.Gender, act => act.MapFrom(src => src.Gender));

            cfg.CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.Username, opt => opt.Ignore())
                .ForMember(dest => dest.Dni, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.Avatar, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Name = src.Name ?? dest.Name;
                    dest.Email = src.Email ?? dest.Email;
                    dest.Username = src.Username ?? dest.Username;
                    dest.Dni = src.Dni ?? dest.Dni;
                    dest.Gender = src.Gender != null ? (Gender)Enum.Parse(typeof(Gender), src.Gender) : dest.Gender;
                    dest.Avatar = src.Avatar ?? dest.Avatar;
                    dest.Password = src.Password .IsNullOrEmpty() ? dest.Password : BCrypt.Net.BCrypt.HashPassword(src.Password);
                    dest.UpdatedAt = DateTime.Now.ToUniversalTime();
                });

            cfg.CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Role, act => act.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Gender, act => act.MapFrom(src => src.Gender.ToString()));
            
            cfg.CreateMap<UserResponseDto, User>()
                .ForMember(dest => dest.Role, act => act.MapFrom(src => Enum.Parse<Roles>(src.Role)));

            /*TRANSACTIONS*/
            cfg.CreateMap<TransactionCreateDto, Transaction>();
            
            cfg.CreateMap<Transaction, TransactionResponseDto>();
            
            /*CARDS*/
            cfg.CreateMap<CardCreateDto, Card>()
                .ForMember(dest => dest.CardNumber, act => act.MapFrom(src => new Random().NextInt64(1000000000000000, 9999999999999999).ToString()))
                .ForMember(dest => dest.Pin, act => act.MapFrom(src => AesOperation.EncryptString("cOtaCSQZGwhAZx3afFShMyNtuEiammK", src.Pin)))
                .ForMember(dest => dest.CardType, act => act.MapFrom(src => Enum.Parse<CardType>(src.CardType)))
                .ForMember(dest => dest.IsBlocked, act => act.MapFrom(src => false))
                .ForMember(dest => dest.ExpirationDate, act => act.MapFrom(src => DateTime.Now.AddYears(4).ToUniversalTime()))
                .ForMember(dest => dest.Cvv, act => act.MapFrom(src => AesOperation.EncryptString("X7V8NMadvSsUsmq391Gw48a", new Random().Next(100, 999).ToString())))
                .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => false))
                .ForMember(dest => dest.User, act => act.Ignore())
                .ForMember(dest => dest.BankAccount, act => act.Ignore());

            cfg.CreateMap<Card, CardResponseDto>()
                .ForMember(dest => dest.CardType, act => act.MapFrom(src => src.CardType.ToString()))
                .ForMember(dest => dest.Cvv,
                    act => act.MapFrom(src => AesOperation.DecryptString("X7V8NMadvSsUsmq391Gw48a", src.Cvv)))
                .ForMember(dest => dest.Pin,
                    act => act.MapFrom(src => AesOperation.DecryptString("cOtaCSQZGwhAZx3afFShMyNtuEiammK", src.Pin)));
            
            cfg.CreateMap<CardUpdateDto, Card>()
                .ForMember(dest => dest.Pin, act => act.Ignore())
                .ForMember(dest => dest.CardType, act => act.Ignore())
                .ForMember(dest => dest.UserId, act => act.Ignore())
                .ForMember(dest => dest.BankAccountIban, act => act.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Pin = src.Pin != null ? AesOperation.EncryptString("cOtaCSQZGwhAZx3afFShMyNtuEiammK", src.Pin) : dest.Pin;
                    dest.CardType = src.CardType != null ? (CardType)Enum.Parse(typeof(CardType), src.CardType) : dest.CardType;
                    dest.UserId = src.UserId ?? dest.UserId;
                    dest.BankAccountIban = src.BankAccountIban ?? dest.BankAccountIban;
                });
        });

        return new Mapper(config);
    }
}
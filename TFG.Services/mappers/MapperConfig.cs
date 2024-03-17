using AutoMapper;
using TFG.Context.DTOs.bankAccount;
using TFG.Context.DTOs.transactions;
using TFG.Context.DTOs.users;
using TFG.Context.Models;

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
                .ForMember(dest => dest.Iban, opt => opt.Ignore())
                .ForMember(dest => dest.Balance, opt => opt.Ignore())
                .ForMember(dest => dest.AccountType, opt => opt.Ignore())
                .ForMember(dest => dest.UsersId, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Iban = src.Iban ?? dest.Iban;
                    dest.Balance = src.Balance ?? dest.Balance;
                    dest.AccountType = src.AccountType ?? dest.AccountType;
                });

            cfg.CreateMap<BankAccount, BankAccountResponseDto>()
                .ForMember(dest => dest.AccountType, act => act.MapFrom(src => src.AccountType.ToString()))
                .ForMember(dest => dest.UsersId, act => act.MapFrom(src => src.UsersId.Select(u => u.Id).ToList()))
                .ForMember(dest => dest.UsersName, act => act.MapFrom(src => src.UsersId.Select(u => u.Name).ToList()));

            /*USERS*/
            cfg.CreateMap<UserCreateDto, User>()
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
                    dest.Password = src.Password ?? dest.Password;
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
        });

        return new Mapper(config);
    }
}
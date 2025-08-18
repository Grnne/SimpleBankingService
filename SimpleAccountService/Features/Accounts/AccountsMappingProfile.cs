using AutoMapper;
using JetBrains.Annotations;
using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts;

[UsedImplicitly]
public class AccountsMappingProfile : Profile
{
    public AccountsMappingProfile()
    {
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.Transactions, opt =>
                opt.MapFrom(src => src.Transactions));

        CreateMap<UpdateAccountDto, Account>()
            .ForMember(dest => dest.Frozen, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.Currency, opt => opt.Ignore())
            .ForMember(dest => dest.Balance, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.LastInterestAccrualAt, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition(
                srcMember => srcMember != null));

        CreateMap<CreateAccountDto, Account>()
            .ForMember(dest => dest.Frozen, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Balance, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore())
            .ForMember(dest => dest.LastInterestAccrualAt, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition(srcMember => srcMember != null));

        CreateMap<Transaction, TransactionForStatementDto>();
    }
}
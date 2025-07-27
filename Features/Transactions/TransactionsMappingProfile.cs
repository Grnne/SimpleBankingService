using AutoMapper;
using JetBrains.Annotations;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Transactions;

[UsedImplicitly]
public class TransactionsMappingProfile : Profile
{
    public TransactionsMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>();

        CreateMap<CreateTransactionDto, Transaction>()
            .ForMember(dest => dest.CounterpartyAccountId, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition(srcMember => srcMember != null));

        CreateMap<TransferDto, Transaction>()
            .ForMember(dest => dest.CounterpartyAccountId, opt => opt.MapFrom(src => src.DestinationAccountId))
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
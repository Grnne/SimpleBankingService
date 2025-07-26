using AutoMapper;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Transactions;

public class TransactionsMappingProfile : Profile
{
    public TransactionsMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>();

        CreateMap<CreateTransactionDto, Transaction>()
            .ForAllMembers(opt => opt.Condition(
                (src, dest, srcMember) => srcMember != null));
    }
}
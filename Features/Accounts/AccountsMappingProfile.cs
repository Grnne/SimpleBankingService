using AutoMapper;
using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Accounts;

public class AccountsMappingProfile : Profile
{
    public AccountsMappingProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<UpdateAccountDto, Account>()
            .ForAllMembers(opt => opt.Condition(
                (src, dest, srcMember) => srcMember != null));

        CreateMap<CreateAccountDto, Account>()
            .ForAllMembers(opt => opt.Condition(
                (src, dest, srcMember) => srcMember != null));

        CreateMap<Transaction, TransactionForStatementDto>();
    }
}
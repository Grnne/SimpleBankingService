using AutoMapper;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Entities;

namespace Simple_Account_Service.Features.Accounts;

public class AccountsMappingProfile : Profile
{
    public AccountsMappingProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<Account, UpdateAccountDto>();

    }
}

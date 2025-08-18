using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccounts;

[UsedImplicitly]
public class GetAllAccountsQueryHandler(IAccountRepository repository, IMapper mapper) : IRequestHandler<GetAllAccountsQuery, MbResult<IEnumerable<AccountDto>>>
{
    public async Task<MbResult<IEnumerable<AccountDto>>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await repository.GetAllAccountsAsync(CancellationToken.None);

        return new MbResult<IEnumerable<AccountDto>>(mapper.Map<IEnumerable<AccountDto>>(accounts));
    }
}
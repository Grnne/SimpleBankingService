using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Accounts.Interfaces;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccounts;

[UsedImplicitly]
public class GetAllAccountsQueryHandler(IAccountsService service) : IRequestHandler<GetAllAccountsQuery, IEnumerable<AccountDto>>
{
    public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        return await service.GetAllAccountsAsync();
    }
}
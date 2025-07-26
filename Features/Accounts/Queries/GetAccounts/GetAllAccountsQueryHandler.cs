using MediatR;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccounts;

public class GetAllAccountsQueryHandler(AccountsService service) : IRequestHandler<GetAllAccountsQuery, IEnumerable<AccountDto>>
{
    public async Task<IEnumerable<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        return await service.GetAllAccountsAsync();
    }
}
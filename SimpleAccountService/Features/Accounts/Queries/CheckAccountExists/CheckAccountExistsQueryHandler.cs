using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Accounts.Interfaces;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

[UsedImplicitly]
public class CheckAccountExistsQueryHandler(IAccountsService service) : IRequestHandler<CheckAccountExistsQuery, bool>
{
    public Task<bool> Handle(CheckAccountExistsQuery request, CancellationToken cancellationToken)
    {
        return service.CheckAccountExists(request.AccountId);
    }
}
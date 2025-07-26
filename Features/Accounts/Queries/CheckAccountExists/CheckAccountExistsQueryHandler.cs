using MediatR;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

public class CheckAccountExistsQueryHandler(AccountsService service) : IRequestHandler<CheckAccountExistsQuery, bool>
{
    public Task<bool> Handle(CheckAccountExistsQuery request, CancellationToken cancellationToken)
    {
        return service.CheckAccountExists(request.AccountId);
    }
}
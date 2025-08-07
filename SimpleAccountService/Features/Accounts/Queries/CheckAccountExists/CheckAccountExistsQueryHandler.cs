using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

[UsedImplicitly]
public class CheckAccountExistsQueryHandler(IAccountRepository repository) : IRequestHandler<CheckAccountExistsQuery, MbResult<bool>>
{
    public async Task<MbResult<bool>> Handle(CheckAccountExistsQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetByIdAsync(request.AccountId);

        return new MbResult<bool>(result != null);
    }
}
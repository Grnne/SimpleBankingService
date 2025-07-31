using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

[UsedImplicitly]
public class GetAccountStatementQueryHandler(IAccountsService service) : IRequestHandler<GetAccountStatementQuery, MultiAccountStatementDto>
{
    public Task<MultiAccountStatementDto> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        return service.GetAccountStatement(request.OwnerId, request.AccountId, request.StartDate, request.EndDate);
    }
}

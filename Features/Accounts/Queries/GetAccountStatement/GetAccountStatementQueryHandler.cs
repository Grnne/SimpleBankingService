using MediatR;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

public class GetAccountStatementQueryHandler(AccountsService service) : IRequestHandler<GetAccountStatementQuery, MultiAccountStatementDto>
{
    public Task<MultiAccountStatementDto> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        return service.GetAccountStatement(request.OwnerId, request.AccountId, request.StartDate, request.EndDate);
    }
}

using MediatR;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

public record GetAccountStatement(Guid Id) : IRequest<Account>;
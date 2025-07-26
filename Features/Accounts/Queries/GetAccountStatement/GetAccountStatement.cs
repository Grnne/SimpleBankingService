using MediatR;
using Simple_Account_Service.Features.Accounts.Entitites;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

public record GetAccountStatement(Guid Id) : IRequest<Account>;
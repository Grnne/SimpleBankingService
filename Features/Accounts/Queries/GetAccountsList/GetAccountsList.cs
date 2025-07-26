using MediatR;
using Simple_Account_Service.Features.Accounts.Entitites;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountsList;

public record GetAccountsList : IRequest<IEnumerable<Account>>;
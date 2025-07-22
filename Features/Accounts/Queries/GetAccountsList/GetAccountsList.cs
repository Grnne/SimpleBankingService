using MediatR;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountsList;

public record GetAccountsList : IRequest<IEnumerable<Account>>;
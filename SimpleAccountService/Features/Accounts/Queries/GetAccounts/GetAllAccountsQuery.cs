using MediatR;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccounts;

public record GetAllAccountsQuery : IRequest<IEnumerable<AccountDto>>;
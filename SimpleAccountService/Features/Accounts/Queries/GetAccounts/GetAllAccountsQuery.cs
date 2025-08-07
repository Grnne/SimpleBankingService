using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccounts;

public record GetAllAccountsQuery : IRequest<MbResult<IEnumerable<AccountDto>>>;
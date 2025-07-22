using MediatR;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountsList;
using Simple_Account_Service.Infrastructure.InMemory;

namespace Simple_Account_Service.Features.Accounts.Handlers;

public class GetAccountsListHandler(FakeDb db) : IRequestHandler<GetAccountsList, IEnumerable<Account>>
{
    public async Task<IEnumerable<Account>> Handle(GetAccountsList request,
        CancellationToken cancellationToken) => await db.GetAllProducts();
}
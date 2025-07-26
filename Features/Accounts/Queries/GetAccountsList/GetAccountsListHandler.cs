using MediatR;
using Simple_Account_Service.Features.Accounts.Entitites;
using Simple_Account_Service.Infrastructure.FakeDb;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountsList;

public class GetAccountsListHandler(FakeDb db) : IRequestHandler<GetAccountsList, IEnumerable<Account>>
{
    public async Task<IEnumerable<Account>> Handle(GetAccountsList request,
        CancellationToken cancellationToken) => await db.GetAllProducts();
}
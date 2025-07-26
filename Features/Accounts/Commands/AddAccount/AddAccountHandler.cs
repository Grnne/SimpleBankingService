using MediatR;
using Simple_Account_Service.Infrastructure.FakeDb;

namespace Simple_Account_Service.Features.Accounts.Commands.AddAccount;

public class AddAccountHandler(FakeDb db) : IRequestHandler<AddAccount>
{
    public async Task Handle(AddAccount request, CancellationToken cancellationToken)
    {
        await db.AddAccount(request.Account);

        return;
    }
}
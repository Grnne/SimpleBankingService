using MediatR;
using Simple_Account_Service.Infrastructure.InMemory;

namespace Simple_Account_Service.Features.Accounts.Commands.AddAccount;

public class AddAccountHandler : IRequestHandler<AddAccount>
{
    private readonly FakeDb _db;

    public AddAccountHandler(FakeDb db)
    {
        _db = db;
    }

    public async Task Handle(AddAccount request, CancellationToken cancellationToken)
    {
        await _db.AddAccount(request.Account);

        return;
    }
}
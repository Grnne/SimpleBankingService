using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Accounts.Interfaces;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

[UsedImplicitly]
public class DeleteAccountCommandHandler(IAccountsService service) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        await service.DeleteAccountAsync(request.AccountId);
    }
}
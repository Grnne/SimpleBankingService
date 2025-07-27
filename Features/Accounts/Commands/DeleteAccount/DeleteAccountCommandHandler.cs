using MediatR;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

public class DeleteAccountCommandHandler(AccountsService service) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        await service.DeleteAccountAsync(request.AccountId);
    }
}
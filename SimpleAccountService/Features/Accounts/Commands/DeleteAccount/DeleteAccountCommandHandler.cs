using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

[UsedImplicitly]
public class DeleteAccountCommandHandler(IAccountRepository repository) : IRequestHandler<DeleteAccountCommand, MbResult<bool>>
{
    public async Task<MbResult<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.AccountId);

        if (account == null)
        {
            throw new NotFoundException($"Счет с айди {request.AccountId} не найден.");
        }

        return new MbResult<bool>(await repository.DeleteAsync(request.AccountId));
    }
}
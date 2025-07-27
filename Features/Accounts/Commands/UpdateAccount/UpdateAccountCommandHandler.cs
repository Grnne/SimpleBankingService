using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Accounts.Interfaces;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

[UsedImplicitly]
public class UpdateAccountCommandHandler(IAccountsService service) : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var response = service.UpdateAccountAsync(request.AccountId, request.Request);

        return await response;
    }
}
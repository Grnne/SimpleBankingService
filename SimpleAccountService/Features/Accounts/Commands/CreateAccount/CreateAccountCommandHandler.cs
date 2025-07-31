using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Accounts.Interfaces;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

[UsedImplicitly]
public class CreateAccountCommandHandler(IAccountsService service) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var response = service.CreateAccountAsync(request.Request);

        return await response;
    }
}
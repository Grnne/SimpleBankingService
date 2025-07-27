using MediatR;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

public class UpdateAccountCommandHandler(AccountsService service) : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var response = service.UpdateAccountAsync(request.AccountId, request.Request);

        return await response;
    }
}
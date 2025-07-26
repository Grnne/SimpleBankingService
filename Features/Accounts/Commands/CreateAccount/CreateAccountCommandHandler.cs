using MediatR;
namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommandHandler(AccountsService service) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var response = service.CreateAccountAsync(request.Request);

        return await response;
    }
}
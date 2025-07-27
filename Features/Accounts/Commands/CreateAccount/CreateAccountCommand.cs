using MediatR;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(CreateAccountDto Request) : IRequest<AccountDto>;
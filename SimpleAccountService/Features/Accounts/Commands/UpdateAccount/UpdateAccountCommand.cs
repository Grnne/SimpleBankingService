using MediatR;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand(Guid AccountId, UpdateAccountDto Request) : IRequest<AccountDto>;

using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;

public record UpdateAccountCommand(Guid AccountId, UpdateAccountDto Request) : IRequest<MbResult<AccountDto>>;

using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(CreateAccountDto Request) : IRequest<MbResult<AccountDto>>;
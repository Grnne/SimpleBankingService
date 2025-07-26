using MediatR;
using Simple_Account_Service.Features.Accounts.Entitites;

namespace Simple_Account_Service.Features.Accounts.Commands.AddAccount;

public record AddAccountValidator(Account Account) : IRequest;
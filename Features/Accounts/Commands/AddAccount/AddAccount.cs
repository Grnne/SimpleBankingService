using MediatR;

namespace Simple_Account_Service.Features.Accounts.Commands.AddAccount;

public record AddAccount(Account Account) : IRequest;
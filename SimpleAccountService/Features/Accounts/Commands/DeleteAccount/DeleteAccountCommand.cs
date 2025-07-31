using MediatR;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid AccountId) : IRequest;

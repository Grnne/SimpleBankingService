using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Accounts.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid AccountId) : IRequest<MbResult<bool>>;

using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

public record CheckAccountExistsQuery(Guid AccountId) : IRequest<MbResult<bool>>;
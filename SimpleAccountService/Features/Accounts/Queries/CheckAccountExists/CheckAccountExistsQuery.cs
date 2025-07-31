using MediatR;

namespace Simple_Account_Service.Features.Accounts.Queries.CheckAccountExists;

public record CheckAccountExistsQuery(Guid AccountId) : IRequest<bool>;
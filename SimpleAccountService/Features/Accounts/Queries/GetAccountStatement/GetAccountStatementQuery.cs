using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

namespace Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement;

public record GetAccountStatementQuery(Guid OwnerId, Guid? AccountId,
    DateTime StartDate, DateTime EndDate) : IRequest<MbResult<MultiAccountStatementDto>>;
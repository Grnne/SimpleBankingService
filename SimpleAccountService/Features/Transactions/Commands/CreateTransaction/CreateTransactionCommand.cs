using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand(Guid AccountId, CreateTransactionDto CreateTransactionDto, 
    Guid CorrelationId) : IRequest<MbResult<TransactionDto>>;

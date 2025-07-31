using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Transactions.Interfaces;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;

[UsedImplicitly]
public class CreateTransactionCommandHandler(ITransactionService service) : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        return service.CreateTransactionAsync(request.AccountId, request.CreateTransactionDto);
    }
}
using MediatR;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandHandler(TransactionService service) : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        return service.CreateTransactionAsync(request.AccountId, request.CreateTransactionDto);
    }
}
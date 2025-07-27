using MediatR;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

public class TransferBetweenAccountsCommandHandler(TransactionService service) : IRequestHandler<TransferBetweenAccountsCommand, List<TransactionDto>>
{
    public async Task<List<TransactionDto>> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        return await service.TransferBetweenAccounts(request.AccountId, request.TransferDto);
    }
}

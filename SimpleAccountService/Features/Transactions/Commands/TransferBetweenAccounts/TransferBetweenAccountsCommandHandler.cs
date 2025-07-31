using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Features.Transactions.Interfaces;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

[UsedImplicitly]
public class TransferBetweenAccountsCommandHandler(ITransactionService service) : IRequestHandler<TransferBetweenAccountsCommand, List<TransactionDto>>
{
    public async Task<List<TransactionDto>> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        return await service.TransferBetweenAccounts(request.AccountId, request.TransferDto);
    }
}
using MediatR;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

public record TransferBetweenAccountsCommand(Guid AccountId, TransferDto TransferDto) : IRequest<List<TransactionDto>>;
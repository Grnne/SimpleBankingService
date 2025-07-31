using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

namespace Simple_Account_Service.Features.Transactions.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateTransactionAsync(Guid accountId, CreateTransactionDto createTransactionDto);

    Task<List<TransactionDto>> TransferBetweenAccounts(Guid accountId, TransferDto transferDto);
}
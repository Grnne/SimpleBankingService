using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

namespace Simple_Account_Service.Features.Transactions.Interfaces;

public interface ITransactionService
{
    Task<MbResult<TransactionDto>> CreateTransactionAsync(Guid accountId, CreateTransactionDto createTransactionDto);

    Task<MbResult<List<TransactionDto>>> TransferBetweenAccounts(Guid accountId, TransferDto transferDto);
}
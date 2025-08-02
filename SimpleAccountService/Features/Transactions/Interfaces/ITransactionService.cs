using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Simple_Account_Service.Features.Transactions.Interfaces;

public interface ITransactionService
{
    void CheckAccount(Guid accountId, [NotNull] Account? account, TransactionType type, decimal amount,
        string transactionCurrency);
}
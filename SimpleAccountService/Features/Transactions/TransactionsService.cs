using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Simple_Account_Service.Features.Transactions;

public class TransactionsService : ITransactionService
{
    public void CheckAccount(Guid accountId, [NotNull] Account? account, TransactionType type, decimal amount, string transactionCurrency)
    {
        if (account == null)
        {
            throw new NotFoundException($"Счет с id {accountId} не найден.");
        }

        if (!string.Equals(account.Currency, transactionCurrency, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                $"Валюта транзакции ({transactionCurrency}) не совпадает с валютой счета ({account.Currency}).");
        }

        if (type == TransactionType.Debit && account.Balance < amount)
        {
            throw new ConflictException("Недостаточно средств для проведения списания.");
        }
    }
}
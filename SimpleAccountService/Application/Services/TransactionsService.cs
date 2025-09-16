using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Simple_Account_Service.Application.Services;

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
                $"Валюта транзакции ({transactionCurrency}) не совпадает с валютой ({account.Currency}) счета ({account.Id}).");
        }

        switch (type)
        {
            case TransactionType.Debit when account.Type != AccountType.Credit && account.Balance < amount:
                throw new ConflictException("Недостаточно средств для проведения списания.");
            case TransactionType.Debit when account.Type == AccountType.Credit && account.Balance + account.CreditLimit < amount:
                throw new ConflictException("Недостаточно средств для проведения списания, кредитный лимит будет превышен.");
            case TransactionType.Credit:
            case TransactionType.Debit:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
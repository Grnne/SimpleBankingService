using AutoMapper;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;
using Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace Simple_Account_Service.Features.Transactions;

public class TransactionsService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IMapper mapper) : ITransactionService
{
    //Пока нет нормальной бд часть бд логики в сервисах.

    public async Task<MbResult<TransactionDto>> CreateTransactionAsync(Guid accountId, CreateTransactionDto createTransactionDto)
    {
        var account = await accountRepository.GetByIdAsync(accountId);
        CheckAccount(accountId, account, createTransactionDto.Type, createTransactionDto.Amount, createTransactionDto.Currency);

        var transaction = mapper.Map<Transaction>(createTransactionDto);

        transaction.Id = Guid.NewGuid();
        transaction.AccountId = accountId;
        transaction.Timestamp = DateTime.UtcNow;

        var result = await transactionRepository.CreateAsync(transaction);

        switch (createTransactionDto.Type)
        {
            case TransactionType.Debit:
                account.Balance -= createTransactionDto.Amount;
                break;
            case TransactionType.Credit:
                account.Balance += createTransactionDto.Amount;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(accountId));
        }
        await accountRepository.UpdateAsync(account);

        return new MbResult<TransactionDto>(mapper.Map<TransactionDto>(result));
    }

    public async Task<MbResult<List<TransactionDto>>> TransferBetweenAccounts(Guid accountId, TransferDto transferDto)
    {
        var sourceAccount = await accountRepository.GetByIdAsync(accountId);
        CheckAccount(accountId, sourceAccount, transferDto.Type, transferDto.Amount, transferDto.Currency);

        var destinationTransactionType = transferDto.Type == TransactionType.Credit
            ? TransactionType.Debit
            : TransactionType.Credit;

        var destinationAccount = await accountRepository.GetByIdAsync(transferDto.DestinationAccountId);

        if (destinationAccount == null)
        {
            throw new NotFoundException($"Счет с id {transferDto.DestinationAccountId} не найден.");
        }

        CheckAccount(destinationAccount.Id, destinationAccount, destinationTransactionType, transferDto.Amount, transferDto.Currency);

        var sourceTransaction = mapper.Map<Transaction>(transferDto);
        sourceTransaction.Id = Guid.NewGuid();
        sourceTransaction.AccountId = accountId;
        sourceTransaction.Timestamp = DateTime.UtcNow;

        var destinationTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = destinationAccount.Id,
            CounterpartyAccountId = accountId,
            Amount = sourceTransaction.Amount,
            Currency = sourceTransaction.Currency,
            Type = destinationTransactionType,
            Timestamp = DateTime.UtcNow
        };

        await transactionRepository.CreateAsync(sourceTransaction);
        await transactionRepository.CreateAsync(destinationTransaction);
        await accountRepository.UpdateAsync(sourceAccount);
        await accountRepository.UpdateAsync(destinationAccount);
        
        // Легкочитаемый кортеж с тернарником для изменения баланса у исходного и корреспондентского счетов
        (sourceAccount.Balance, destinationAccount.Balance) = sourceTransaction.Type == TransactionType.Debit
            ? (sourceAccount.Balance - sourceTransaction.Amount, destinationAccount.Balance + sourceTransaction.Amount)
            : (sourceAccount.Balance + sourceTransaction.Amount, destinationAccount.Balance - sourceTransaction.Amount);

        return new MbResult<List<TransactionDto>>(
            [mapper.Map<TransactionDto>(sourceTransaction), mapper.Map<TransactionDto>(destinationTransaction)]);
    }

    private static void CheckAccount(Guid accountId, [NotNull] Account? account, TransactionType type, decimal amount, string transactionCurrency)
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
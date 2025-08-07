using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

[UsedImplicitly]
public class TransferBetweenAccountsCommandHandler(ITransactionService service, ITransactionRepository transactionRepository, IAccountRepository accountRepository, IMapper mapper) : IRequestHandler<TransferBetweenAccountsCommand, MbResult<List<TransactionDto>>>
{
    public async Task<MbResult<List<TransactionDto>>> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        var accountId = request.AccountId;
        var transferDto = request.TransferDto;

        var sourceAccount = await accountRepository.GetByIdAsync(accountId);
        service.CheckAccount(accountId, sourceAccount, transferDto.Type, transferDto.Amount, transferDto.Currency);

        var destinationTransactionType = transferDto.Type == TransactionType.Credit
            ? TransactionType.Debit
            : TransactionType.Credit;

        var destinationAccount = await accountRepository.GetByIdAsync(transferDto.DestinationAccountId);

        if (destinationAccount == null)
        {
            throw new NotFoundException($"Счет с id {transferDto.DestinationAccountId} не найден.");
        }

        service.CheckAccount(destinationAccount.Id, destinationAccount, destinationTransactionType, transferDto.Amount, transferDto.Currency);

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
}
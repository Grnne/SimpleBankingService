using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Events;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;
using System.Data;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

[UsedImplicitly]
public class TransferBetweenAccountsCommandHandler(
    SasDbContext context, ITransactionService service, ITransactionRepository transactionRepository, IAccountRepository accountRepository,
    IMapper mapper, IMediator mediator, ILogger<TransferBetweenAccountsCommandHandler> logger)
    : IRequestHandler<TransferBetweenAccountsCommand, MbResult<List<TransactionDto>>>
{
    public async Task<MbResult<List<TransactionDto>>> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        var correlationId = request.CorrelationId;

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
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

            (sourceAccount.Balance, destinationAccount.Balance) = sourceTransaction.Type == TransactionType.Debit
                ? (sourceAccount.Balance - sourceTransaction.Amount, destinationAccount.Balance + sourceTransaction.Amount)
                : (sourceAccount.Balance + sourceTransaction.Amount, destinationAccount.Balance - sourceTransaction.Amount);

            await transactionRepository.CreateAsync(sourceTransaction);
            await transactionRepository.CreateAsync(destinationTransaction);
            await accountRepository.UpdateAsync(sourceAccount);
            await accountRepository.UpdateAsync(destinationAccount);

            var debitTransaction = sourceTransaction;
            var creditTransaction = destinationTransaction;
            if (debitTransaction.Type != TransactionType.Debit)
            {
                (debitTransaction, creditTransaction) = (creditTransaction, debitTransaction);
            }

            var causationId = correlationId;
            await mediator.Publish(new MoneyDebited(
                Transaction: debitTransaction,
                Source: "transactions",
                CorrelationId: correlationId,
                CausationId: causationId,
                Reason: debitTransaction.Description), cancellationToken);
            causationId = Guid.NewGuid();
            logger.LogInformation("Published MoneyDebited event with CorrelationId: {CorrelationId} and CausationId: {CausationId}", correlationId, causationId);

            await mediator.Publish(new MoneyCredited(
                Transaction: creditTransaction,
                Source: "transactions",
                CorrelationId: correlationId,
                CausationId: causationId), cancellationToken);
            causationId = Guid.NewGuid();
            logger.LogInformation("Published MoneyCredited event with CorrelationId: {CorrelationId} and CausationId: {CausationId}", correlationId, causationId);

            await mediator.Publish(new TransferCompleted(
                SourceAccountId: sourceAccount.Id,
                DestinationAccountId: destinationAccount.Id,
                Amount: sourceTransaction.Amount,
                Currency: sourceTransaction.Currency,
                TransferSourceId: sourceTransaction.Id,
                TransferDestinationId: destinationTransaction.Id,
                Source: "transactions",
                CorrelationId: correlationId,
                CausationId: causationId), cancellationToken);
            logger.LogInformation("Published TransferCompleted event with CorrelationId: {CorrelationId} and CausationId: {CausationId}", correlationId, causationId);

            await transaction.CommitAsync(cancellationToken);

            var resultList = new List<TransactionDto> { mapper.Map<TransactionDto>(sourceTransaction), mapper.Map<TransactionDto>(destinationTransaction) };
            return new MbResult<List<TransactionDto>>(resultList);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error in TransferBetweenAccountsCommandHandler, CorrelationId: {CorrelationId}", request.CorrelationId);
            throw;
        }
    }

}
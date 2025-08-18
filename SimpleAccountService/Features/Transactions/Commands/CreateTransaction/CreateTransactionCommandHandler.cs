using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Events;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Features.Transactions.Commands.CreateTransaction;

[UsedImplicitly]
public class CreateTransactionCommandHandler(SasDbContext context, ITransactionRepository transactionRepository,
    IAccountRepository accountRepository, ITransactionService service, IMapper mapper,    IMediator mediator,
    ILogger<CreateTransactionCommandHandler> logger)
    : IRequestHandler<CreateTransactionCommand, MbResult<TransactionDto>>
{
    public async Task<MbResult<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Start handling CreateTransactionCommand, CorrelationId: {CorrelationId}",
            request.CorrelationId);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = await accountRepository.GetByIdAsync(request.AccountId);
            service.CheckAccount(request.AccountId, account, request.CreateTransactionDto.Type, request.CreateTransactionDto.Amount, request.CreateTransactionDto.Currency);

            var accountTransaction = mapper.Map<Transaction>(request.CreateTransactionDto);
            accountTransaction.Id = Guid.NewGuid();
            accountTransaction.AccountId = request.AccountId;
            accountTransaction.Timestamp = DateTime.UtcNow;

            var result = await transactionRepository.CreateAsync(accountTransaction);

            switch (request.CreateTransactionDto.Type)
            {
                case TransactionType.Debit:
                    account.Balance -= request.CreateTransactionDto.Amount;
                    break;
                case TransactionType.Credit:
                    account.Balance += request.CreateTransactionDto.Amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }

            if (request.CreateTransactionDto.Type == TransactionType.Credit)
            {
                await mediator.Publish(new MoneyCredited(
                    Transaction: accountTransaction,
                    Source: "transactions",
                    CorrelationId: request.CorrelationId,
                    CausationId: Guid.NewGuid()
                ), cancellationToken);

                logger.LogInformation("Published MoneyCredited event, CorrelationId: {CorrelationId}, TransactionId: {TransactionId}",
                    request.CorrelationId, accountTransaction.Id);
            }
            else
            {
                await mediator.Publish(new MoneyDebited(
                    Transaction: accountTransaction,
                    Source: "transactions",
                    CorrelationId: request.CorrelationId,
                    CausationId: Guid.NewGuid(),
                    Reason: accountTransaction.Description
                ), cancellationToken);

                logger.LogInformation("Published MoneyDebited event, CorrelationId: {CorrelationId}, TransactionId: {TransactionId}",
                    request.CorrelationId, accountTransaction.Id);
            }

            await accountRepository.UpdateAsync(account);
            await transaction.CommitAsync(cancellationToken);

            return new MbResult<TransactionDto>(mapper.Map<TransactionDto>(result));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error handling CreateTransactionCommand, CorrelationId: {CorrelationId}", request.CorrelationId);
            throw;
        }
    }
}

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
public class CreateTransactionCommandHandler(SasDbContext context, ITransactionRepository transactionRepository, IAccountRepository accountRepository, ITransactionService service,
    IMapper mapper, IMediator mediator) : IRequestHandler<CreateTransactionCommand, MbResult<TransactionDto>>
{
    public async Task<MbResult<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var accountId = request.AccountId;
        var createTransactionDto = request.CreateTransactionDto;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var account = await accountRepository.GetByIdAsync(accountId);
            service.CheckAccount(accountId, account, createTransactionDto.Type, createTransactionDto.Amount, createTransactionDto.Currency);

            var accountTransaction = mapper.Map<Transaction>(createTransactionDto);
            accountTransaction.Id = Guid.NewGuid();
            accountTransaction.AccountId = accountId;
            accountTransaction.Timestamp = DateTime.UtcNow;

            var result = await transactionRepository.CreateAsync(accountTransaction);

            switch (createTransactionDto.Type)
            {
                case TransactionType.Debit:
                    account.Balance -= createTransactionDto.Amount;
                    break;
                case TransactionType.Credit:
                    account.Balance += createTransactionDto.Amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }

            if (createTransactionDto.Type == TransactionType.Credit)
            {
                await mediator.Publish(new MoneyCredited(
                    Transaction: accountTransaction,
                    Source: "transactions",
                    CorrelationId: Guid.NewGuid(),
                    CausationId: Guid.NewGuid()
                ), cancellationToken);
            }
            else
            {
                await mediator.Publish(new MoneyDebited(
                    Transaction: accountTransaction,
                    Source: "transactions",
                    CorrelationId: Guid.NewGuid(),
                    CausationId: Guid.NewGuid(),
                    Reason: accountTransaction.Description
                ), cancellationToken);
            }

            await accountRepository.UpdateAsync(account);
            await transaction.CommitAsync(cancellationToken);

            return new MbResult<TransactionDto>(mapper.Map<TransactionDto>(result));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
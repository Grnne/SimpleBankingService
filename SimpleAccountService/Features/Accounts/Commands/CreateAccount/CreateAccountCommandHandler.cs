using AutoMapper;
using JetBrains.Annotations;
using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

[UsedImplicitly]
public class CreateAccountCommandHandler(SasDbContext context, IAccountRepository repository, IMapper mapper,
    IMediator mediator, ILogger<CreateAccountCommandHandler> logger) : IRequestHandler<CreateAccountCommand, MbResult<AccountDto>>
{
    public async Task<MbResult<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Start handling CreateAccountCommand, CorrelationId: {CorrelationId}", request.CorrelationId);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var account = mapper.Map<Account>(request.Request);

            account.Id = Guid.NewGuid();
            account.Balance = 0;
            account.CreatedAt = DateTime.UtcNow;
            account.Transactions = [];

            var response = await repository.CreateAsync(account);

            const string source = "accounts";
            var causationId = Guid.NewGuid();

            await mediator.Publish(new AccountOpened(response, source, request.CorrelationId, causationId), cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Account opened event published, CorrelationId: {CorrelationId}, AccountId: {AccountId}", request.CorrelationId, response.Id);

            return new MbResult<AccountDto>(mapper.Map<AccountDto>(response));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error handling CreateAccountCommand, CorrelationId: {CorrelationId}", request.CorrelationId);
            throw;
        }
    }
}
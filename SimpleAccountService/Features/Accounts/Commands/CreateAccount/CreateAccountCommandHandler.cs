using AutoMapper;
using JetBrains.Annotations;
using MassTransit;
using MediatR;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Features.Accounts.Commands.CreateAccount;

[UsedImplicitly]
public class CreateAccountCommandHandler(SasDbContext context, IAccountRepository repository, IMapper mapper, IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateAccountCommand, MbResult<AccountDto>>
{
    public async Task<MbResult<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
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
            var correlationId = Guid.NewGuid(); // TODO в контроллер
            var causationId = Guid.NewGuid();

            await publishEndpoint.Publish(new AccountOpened(response, source, correlationId, causationId), cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new MbResult<AccountDto>(mapper.Map<AccountDto>(response));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
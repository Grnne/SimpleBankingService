using JetBrains.Annotations;
using Simple_Account_Service.Application.Abstractions;
using Simple_Account_Service.Application.Interfaces.Messaging;
using Simple_Account_Service.Application.Interfaces.Repositories;
using Simple_Account_Service.Features.Accounts.Entities;

namespace Simple_Account_Service.Features.Accounts.Events;

public record AccountOpened(Account Account, string Source, Guid CorrelationId, Guid CausationId) : IOutboxEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid AccountId { get; init; } = Account.Id;
    public Guid OwnerId { get; init; } = Account.OwnerId;
    public string Currency { get; init; } = Account.Currency;
    public decimal? CreditLimit { get; init; } = Account.CreditLimit;
    public AccountType Type { get; init; } = Account.Type;
}
[UsedImplicitly]
public class AccountOpenedHandler(IOutboxRepository repository, ILogger<BaseOutboxEventHandler<AccountOpened, object>> logger)
    : BaseOutboxEventHandler<AccountOpened, object>(repository, logger)
{
    protected override object MapPayload(AccountOpened outboxEvent)
    {
        return new
        {
            accountId = outboxEvent.AccountId,
            ownerId = outboxEvent.OwnerId,
            currency = outboxEvent.Currency,
            creditLimit = outboxEvent.CreditLimit,
            type = outboxEvent.Type
        };
    }
}
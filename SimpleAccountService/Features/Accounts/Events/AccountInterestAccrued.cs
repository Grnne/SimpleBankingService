using JetBrains.Annotations;
using Simple_Account_Service.Application.Abstractions;
using Simple_Account_Service.Application.Interfaces.Messaging;
using Simple_Account_Service.Application.Interfaces.Repositories;

namespace Simple_Account_Service.Features.Accounts.Events;

public record AccountInterestAccrued(
    Guid AccountId,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    decimal Amount,
    string Source,
    Guid CorrelationId,
    Guid CausationId) : IOutboxEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
[UsedImplicitly]
public class AccountInterestAccruedHandler(IOutboxRepository repository,
    ILogger<BaseOutboxEventHandler<AccountInterestAccrued, object>> logger)
    : BaseOutboxEventHandler<AccountInterestAccrued, object>(repository, logger)
{
    protected override object MapPayload(AccountInterestAccrued outboxEvent)
    {
        return new
        {
            accountId = outboxEvent.AccountId,
            periodFrom = outboxEvent.PeriodFrom,
            periodTo = outboxEvent.PeriodTo,
            amount = outboxEvent.Amount
        };
    }
}
using Simple_Account_Service.Application.Interfaces;

namespace Simple_Account_Service.Features.Accounts.Events;

public record InterestAccrued(
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
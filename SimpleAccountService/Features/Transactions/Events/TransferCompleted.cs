using Simple_Account_Service.Application.Interfaces;

namespace Simple_Account_Service.Features.Transactions.Events;

public record TransferCompleted(Guid SourceAccountId, Guid DestinationAccountId, decimal Amount,
    string Currency, Guid TransferSourceId, Guid TransferDestinationId, string Source,
    Guid CorrelationId, Guid CausationId) : IOutboxEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
using Simple_Account_Service.Application.Abstractions;
using Simple_Account_Service.Application.Interfaces;

namespace Simple_Account_Service.Features.Transactions.Events;

public record TransferCompleted(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Currency,
    Guid TransferSourceId,
    Guid TransferDestinationId,
    string Source,
    Guid CorrelationId,
    Guid CausationId) : IOutboxEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public class TransferCompletedHandler(IOutboxRepository repository,
    ILogger<BaseOutboxEventHandler<TransferCompleted, object>> logger)
    : BaseOutboxEventHandler<TransferCompleted, object>(repository, logger)
{
    protected override object MapPayload(TransferCompleted outboxEvent)
    {
        return new
        {
            sourceAccountId = outboxEvent.SourceAccountId,
            destinationAccountId = outboxEvent.DestinationAccountId,
            amount = outboxEvent.Amount,
            currency = outboxEvent.Currency,
            transferSourceId = outboxEvent.TransferSourceId,
            transferDestinationId = outboxEvent.TransferDestinationId
        };
    }
}
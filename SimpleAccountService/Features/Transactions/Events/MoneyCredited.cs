using JetBrains.Annotations;
using Simple_Account_Service.Application.Abstractions;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Transactions.Events;

public record MoneyCredited(Transaction Transaction, string Source, Guid CorrelationId, Guid CausationId) : IOutboxEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid AccountId { get; init; } = Transaction.AccountId;
    public decimal Amount { get; init; } = Transaction.Amount;
    public string Currency { get; init; } = Transaction.Currency;
    public Guid OperationId { get; init; } = Transaction.Id;
}

[UsedImplicitly]
public class MoneyCreditedHandler(IOutboxRepository repository, ILogger<BaseOutboxEventHandler<MoneyCredited, object>> logger)
    : BaseOutboxEventHandler<MoneyCredited, object>(repository, logger)
{
    protected override object MapPayload(MoneyCredited outboxEvent)
    {
        return new
        {
            accountId = outboxEvent.AccountId,
            amount = outboxEvent.Amount,
            currency = outboxEvent.Currency,
            operationId = outboxEvent.OperationId
        };
    }
}
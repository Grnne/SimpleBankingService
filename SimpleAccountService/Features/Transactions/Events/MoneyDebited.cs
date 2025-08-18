using Simple_Account_Service.Application.Abstractions;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Features.Transactions.Entities;
using System;

namespace Simple_Account_Service.Features.Transactions.Events;

public record MoneyDebited(Transaction Transaction, string Source, Guid CorrelationId, Guid CausationId, string? Reason) : IOutboxEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid AccountId { get; init; } = Transaction.AccountId;
    public decimal Amount { get; init; } = Transaction.Amount;
    public string Currency { get; init; } = Transaction.Currency;
    public Guid OperationId { get; init; } = Transaction.Id;
    public string? Reason { get; init; } = Reason;
}

public class MoneyDebitedHandler(IOutboxRepository repository)
    : BaseOutboxEventHandler<MoneyDebited, object>(repository)
{
    protected override object MapPayload(MoneyDebited outboxEvent)
    {
        return new
        {
            accountId = outboxEvent.AccountId,
            amount = outboxEvent.Amount,
            currency = outboxEvent.Currency,
            operationId = outboxEvent.OperationId,
            reason = outboxEvent.Reason ?? "Причина не указана"
        };
    }
}
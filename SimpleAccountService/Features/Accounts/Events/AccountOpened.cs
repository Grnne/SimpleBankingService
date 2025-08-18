using Simple_Account_Service.Application.Interfaces;
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
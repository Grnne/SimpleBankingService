using MediatR;

namespace Simple_Account_Service.Features.Accounts.Events;

public class AccountOpened : INotification
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public Guid AccountId { get; init; }
    public Guid OwnerId { get; init; }
    public string Currency { get; init; } = default!;
    public string Type { get; init; } = default!;
}

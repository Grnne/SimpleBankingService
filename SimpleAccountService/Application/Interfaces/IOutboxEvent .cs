using MediatR;

namespace Simple_Account_Service.Application.Interfaces;

public interface IOutboxEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string Source { get; }
    Guid CorrelationId { get; }
    Guid CausationId { get; }
}
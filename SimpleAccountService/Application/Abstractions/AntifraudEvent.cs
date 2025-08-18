using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Application.Abstractions;


public abstract record AntifraudEvent(
    Guid EventId,
    DateTime OccurredAt,
    EventMeta Meta
) : INotification;
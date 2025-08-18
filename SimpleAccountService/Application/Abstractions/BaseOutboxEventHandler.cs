using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Infrastructure.Messaging.Outbox;

namespace Simple_Account_Service.Application.Abstractions;

public abstract class BaseOutboxEventHandler
{
    protected static readonly JsonSerializerOptions SerializeOptions = new()
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}

public abstract class BaseOutboxEventHandler<TEvent, TPayload>(IOutboxRepository repository, ILogger<BaseOutboxEventHandler<TEvent, TPayload>> logger)
    : BaseOutboxEventHandler, INotificationHandler<TEvent>
    where TEvent : IOutboxEvent
{

    protected abstract TPayload MapPayload(TEvent outboxEvent);

    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling outbox event {EventType} with EventId {EventId} CorrelationId {CorrelationId}",
            typeof(TEvent).Name, notification.EventId, notification.CorrelationId);

        var stopwatch = Stopwatch.StartNew();


        var payload = MapPayload(notification);

        if (payload == null)
        {
            var errorMsg = $"Payload for event {typeof(TEvent).Name} cannot be null.";
            logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        var envelope = new EventEnvelope
        {
            EventId = notification.EventId,
            OccurredAt = notification.OccurredAt,
            Meta = new EventMeta
            {
                Version = "v1",
                Source = notification.Source,
                CorrelationId = notification.CorrelationId,
                CausationId = notification.CausationId
            },
            Payload = payload
        };

        var envelopeJson = JsonSerializer.Serialize(envelope, SerializeOptions);

        var outboxMessage = new OutboxMessage
        {
            Id = notification.EventId,
            EventType = typeof(TEvent).Name,
            Payload = envelopeJson,
            OccurredAt = notification.OccurredAt,
            Published = false,
            // Дубликаты для заголовков брокера
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            Source = envelope.Source,
            Version = envelope.Version
        };

        await repository.AddAsync(outboxMessage, cancellationToken);

        stopwatch.Stop();
        logger.LogInformation("Outbox message for event {EventId} saved successfully in {ElapsedMilliseconds} ms",
            notification.EventId, stopwatch.ElapsedMilliseconds);
    }
}
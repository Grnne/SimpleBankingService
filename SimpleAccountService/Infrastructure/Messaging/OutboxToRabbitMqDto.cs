using RabbitMQ.Client;

namespace Simple_Account_Service.Infrastructure.Messaging;

public class OutboxToRabbitMqDto(Guid id, string eventType, byte[] body, BasicProperties props,
    Guid correlationId, Guid causationId)
{
    public Guid Id { get; set; } = id;
    public string EventType { get; set; } = eventType;
    public byte[] Body { get; set; } = body;
    public BasicProperties Props { get; set; } = props;
    public Guid CorrelationId { get; set; } = correlationId;
    public Guid CausationId { get; set; } = causationId;
}
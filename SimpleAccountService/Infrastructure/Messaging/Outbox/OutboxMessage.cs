namespace Simple_Account_Service.Infrastructure.Messaging.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public bool Published { get; set; } = false;

    // Дубликаты колонок для заголовков брокера
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public string Source { get; set; } = "";
    public string Version { get; set; } = "";
}

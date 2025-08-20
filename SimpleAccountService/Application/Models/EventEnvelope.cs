namespace Simple_Account_Service.Application.Models;

public class EventEnvelope
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public EventMeta Meta { get; set; } = new();

    public object Payload { get; set; } = null!;

    // Дубликат для доступа в диспетчере (cringe)
    public Guid CorrelationId => Meta.CorrelationId;
    public Guid CausationId => Meta.CausationId;
    public string Source => Meta.Source;
    public string Version => Meta.Version;
}


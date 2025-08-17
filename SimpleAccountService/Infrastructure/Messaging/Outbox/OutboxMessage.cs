namespace Simple_Account_Service.Infrastructure.Messaging.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public bool Published { get; set; } = false;
}

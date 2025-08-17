namespace Simple_Account_Service.Infrastructure.Messaging.Inbox;

public class InboxConsumedMessage
{
    public Guid MessageId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Handler { get; set; } = null!;
}
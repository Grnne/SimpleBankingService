using System.ComponentModel.DataAnnotations;

namespace Simple_Account_Service.Infrastructure.Messaging.Inbox;

public class InboxDeadLetter
{
    public Guid MessageId { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string Handler { get; set; } = null!;
    [MaxLength(5000)]
    public string Payload { get; set; } = null!;
    [MaxLength(500)]
    public string Error { get; set; } = null!;
    public Guid? CorrelationId { get; set; }
    public Guid? CausationId { get; set; }
}
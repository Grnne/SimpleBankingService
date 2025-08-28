namespace Simple_Account_Service.Infrastructure.Messaging;

public class FromRabbitMqToOutboxDto
{
    public int Id { get; set; }
    public DateTime ProcessedAt { get; set; }
}
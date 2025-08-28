using Simple_Account_Service.Infrastructure.Messaging;

namespace Simple_Account_Service.Application.Interfaces.Messaging;


public interface IRabbitMqPublisher
{
    Task InitAsync();
    Task<List<(Guid, DateTime)>> PublishMessagesReturnPublishedIds(List<OutboxToRabbitMqDto> messages, CancellationToken cancellationToken);
}
using Simple_Account_Service.Infrastructure.Messaging;

namespace Simple_Account_Service.Application.Interfaces.Messaging;

public interface IOutboxDispatcher
{
    Task<IEnumerable<OutboxToRabbitMqDto>> GetMessagesForDispatch(CancellationToken stoppingToken);
    Task MarkMultipleAsProcessed(List<(Guid id, DateTime dateTime)> messageTuples, CancellationToken cancellationToken);
}

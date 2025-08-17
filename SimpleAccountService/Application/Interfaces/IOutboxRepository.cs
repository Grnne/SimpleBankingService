using Simple_Account_Service.Infrastructure.Messaging.Outbox;

namespace Simple_Account_Service.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);
        Task<IEnumerable<OutboxMessage>?> GetUnprocessedAsync(CancellationToken cancellationToken);
        Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken);
    }
}
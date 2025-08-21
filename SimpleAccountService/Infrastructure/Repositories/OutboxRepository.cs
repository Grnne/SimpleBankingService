using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Messaging.Outbox;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class OutboxRepository(SasDbContext context) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await context.Set<OutboxMessage>().AddAsync(message, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>?> GetUnprocessedAsync(CancellationToken cancellationToken)
    {
        return await context.Set<OutboxMessage>()
            .Where(x => !x.Published)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var message = await context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

        if (message != null)
        {
            message.Published = true;
            message.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
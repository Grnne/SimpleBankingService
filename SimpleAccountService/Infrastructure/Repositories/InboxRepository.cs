using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Messaging.Inbox;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class InboxRepository(SasDbContext context) : IInboxRepository
{
    public async Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await context.InboxConsumedMessages
            .AsNoTracking()
            .AnyAsync(x => x.MessageId == eventId, cancellationToken);
    }

    public async Task AddProcessedAsync(Guid eventId, string handler, CancellationToken cancellationToken)
    {
        var entity = new InboxConsumedMessage
        {
            Handler = handler ?? "",
            MessageId = eventId,
            ProcessedAt = DateTime.UtcNow
        };

        await context.InboxConsumedMessages.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
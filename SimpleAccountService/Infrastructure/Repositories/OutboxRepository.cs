using Microsoft.EntityFrameworkCore;
using Npgsql;
using Simple_Account_Service.Application.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Messaging.Outbox;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class OutboxRepository(SasDbContext context) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await context.OutboxMessages.AddAsync(message, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>?> GetUnprocessedAsync(CancellationToken cancellationToken)
    {
        return await context.OutboxMessages
            .Where(x => !x.Published)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var message = await context.OutboxMessages
            .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

        if (message != null)
        {
            message.Published = true;
            message.PublishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkMultipleAsProcessedAsync(List<(Guid id, DateTime dateTime)> messageTuples, CancellationToken cancellationToken)
    {
        if (!messageTuples.Any())
        {
            return;
        }

        var parameters = new List<NpgsqlParameter>();
        var values = new List<string>();

        for (var i = 0; i < messageTuples.Count; i++)
        {
            var (id, publishedAt) = messageTuples[i];
            values.Add($"(@id{i}, @timestamp{i})");
            parameters.Add(new NpgsqlParameter($"id{i}", id));
            parameters.Add(new NpgsqlParameter($"timestamp{i}", publishedAt));
        }

        var sql = $@"
        UPDATE outbox_messages om
        SET 
            published = true, 
            published_at = data.timestamp
        FROM (VALUES {string.Join(", ", values)}) as data(id, timestamp)
        WHERE om.id = data.id::uuid";

        await context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }
}
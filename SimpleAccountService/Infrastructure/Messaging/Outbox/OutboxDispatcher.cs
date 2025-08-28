using RabbitMQ.Client;
using System.Text;
using Simple_Account_Service.Application.Interfaces.Messaging;
using Simple_Account_Service.Application.Interfaces.Repositories;

namespace Simple_Account_Service.Infrastructure.Messaging.Outbox;

public class OutboxDispatcher(IOutboxRepository repository, ILogger<OutboxDispatcher> logger) : IOutboxDispatcher
{
    
    public async Task<IEnumerable<OutboxToRabbitMqDto>> GetMessagesForDispatch(CancellationToken stoppingToken)
    {
        var messages = (await repository.GetUnprocessedAsync(stoppingToken) ?? []).ToList();

        if (messages.Count == 0)
        {
            logger.LogInformation("No outbox messages to dispatch");

            return [];
        }

        var messagesToDispatch = new List<OutboxToRabbitMqDto>();
        logger.LogInformation("Preparing {Count} messages for dispatch", messages.Count);

        foreach (var outboxMessage in messages)
        {
            var body = Encoding.UTF8.GetBytes(outboxMessage.Payload);
            var props = new BasicProperties
            {
                MessageId = outboxMessage.Id.ToString(),
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    { "x-correlation-id", outboxMessage.CorrelationId.ToString() },
                    { "x-causation-id", outboxMessage.CausationId.ToString() },
                    { "x-source", outboxMessage.Source },
                    { "x-version", outboxMessage.Version }
                }
            };

            messagesToDispatch.Add(new OutboxToRabbitMqDto(
                outboxMessage.Id,
                outboxMessage.EventType,
                body,
                props,
                outboxMessage.CorrelationId,
                outboxMessage.CausationId
                ));
        }

        return messagesToDispatch;
    }

    public async Task MarkMultipleAsProcessed(List<(Guid id, DateTime dateTime)> messageTuples, CancellationToken cancellationToken)
    {
        await repository.MarkMultipleAsProcessedAsync(messageTuples,  cancellationToken);
        logger.LogInformation("Marked {Count} messages as processed", messageTuples.Count);
    }
}
using Simple_Account_Service.Application.Interfaces.Messaging;

namespace Simple_Account_Service.Application.Services.Messaging;

public class OutboxProducerService(ILogger<OutboxProducerService> logger, IServiceScopeFactory factory) : BackgroundService
{
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Producer Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = factory.CreateScope())
            {
                try
                {
                    var provider = scope.ServiceProvider;
                    var publisher = provider.GetRequiredService<IRabbitMqPublisher>();
                    var outboxDispatcher = provider.GetRequiredService<IOutboxDispatcher>();

                    await publisher.InitAsync();

                    var messages = await outboxDispatcher.GetMessagesForDispatch(stoppingToken);
                    var messagesList = messages.ToList();

                    if (messagesList.Count > 0)
                    {
                        logger.LogInformation("Dispatching {Count} messages to RabbitMQ", messagesList.Count);

                        var publishedMessages =
                            await publisher.PublishMessagesReturnPublishedIds(messagesList, stoppingToken);

                        if (publishedMessages.Count > 0)
                        {
                            await outboxDispatcher.MarkMultipleAsProcessed(publishedMessages, stoppingToken);
                            logger.LogInformation("Successfully processed {Count} messages", publishedMessages.Count);
                        }
                    }
                    else
                    {
                        logger.LogDebug("No messages to dispatch, waiting for next polling interval");
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Producer Service stopping due to cancellation");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while processing outbox messages");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }
            }
            await Task.Delay(_pollingInterval, stoppingToken);
        }

        logger.LogInformation("Producer Service stopped");
    }
}
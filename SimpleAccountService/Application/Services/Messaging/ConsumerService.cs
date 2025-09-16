using System.Text;
using System.Text.Json;
using MediatR;
using Simple_Account_Service.Application.Interfaces.Messaging;
using Simple_Account_Service.Features.Accounts.Events;

namespace Simple_Account_Service.Application.Services.Messaging;

public class ConsumerService(
    IServiceScopeFactory scopeFactory,
    ILogger<ConsumerService> logger)
    : BackgroundService
{
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(15);

    private readonly Dictionary<string, Type> _routingKeyToTypeMap = new()
    {
        ["client.blocked"] = typeof(ClientBlocked),
        ["client.unblocked"] = typeof(ClientUnblocked)
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consumer Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IRabbitMqConsumer>();

            try
            {
                await consumer.StartConsumingAsync(
                    "account.antifraud",
                    async (body, routingKey, headers) =>
                        await ProcessMessageAsync(body, routingKey, headers, scope.ServiceProvider, stoppingToken),
                    stoppingToken);

                logger.LogInformation("Consumer started successfully. Waiting for {Period}...", _pollingInterval);
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Consumption operation cancelled");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in consumption cycle. Retrying in {Period}...", _pollingInterval);
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            finally
            {
                await consumer.StopConsumingAsync(stoppingToken);
            }
        }

        logger.LogInformation("Consumer Service stopping");
    }

    private async Task<bool> ProcessMessageAsync(
        byte[] body,
        string routingKey,
        IDictionary<string, object> headers,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var bodyString = Encoding.UTF8.GetString(body);

            logger.LogDebug("Received message. RoutingKey: {RoutingKey}", routingKey);

            if (!_routingKeyToTypeMap.TryGetValue(routingKey, out var messageType))
            {
                logger.LogWarning("Unsupported routing key: {RoutingKey}", routingKey);
                return false;
            }

            var message = JsonSerializer.Deserialize(bodyString, messageType,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (message is null)
            {
                logger.LogError("Failed to deserialize {MessageType} for routing key: {RoutingKey}",
                    messageType.Name, routingKey);
                return false;
            }

            await mediator.Publish(message, cancellationToken);

            logger.LogInformation("Message processed successfully. RoutingKey: {RoutingKey}, Event: {EventType}",
                routingKey, messageType.Name);

            return true;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization failed for routing key: {RoutingKey}", routingKey);
            return false;
        }
        catch (NotSupportedException ex)
        {
            logger.LogWarning(ex, "Unsupported routing key: {RoutingKey}", routingKey);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing message. RoutingKey: {RoutingKey}", routingKey);
            return false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Consumer Service performing graceful shutdown...");
        await base.StopAsync(cancellationToken);
    }
}
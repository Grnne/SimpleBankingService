using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Simple_Account_Service.Application.Interfaces.Messaging;

namespace Simple_Account_Service.Infrastructure.Messaging.RabbitMq;

public class RabbitMqConsumer(
    RabbitMqConnectionFactory factory,
    ILogger<RabbitMqConsumer> logger)
    : IRabbitMqConsumer, IAsyncDisposable
{
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;
    private bool _disposed;
    private string? _currentQueueName;
    private Func<byte[], string, IDictionary<string, object>, Task<bool>>? _currentMessageHandler;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public async Task StartConsumingAsync(
        string queueName,
        Func<byte[], string, IDictionary<string, object>, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default)
    {
        _currentQueueName = queueName;
        _currentMessageHandler = messageHandler;

        await EnsureChannelConnectedAsync(cancellationToken);

        if (_channel == null)
        {
            throw new InvalidOperationException("Failed to initialize RabbitMQ channel");
        }

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += async (sender, args) =>
        {
            var success = await HandleMessageAsync(args, messageHandler);
            await HandleAcknowledgmentAsync(args.DeliveryTag, success, cancellationToken);
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: _consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation("Started consuming from queue: {QueueName}", queueName);
    }

    private async Task EnsureChannelConnectedAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true }) return;

        await _channelLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true }) return;

            logger.LogInformation("(Re)initializing RabbitMQ channel...");
            _channel = await factory.GetChannelAsync();

            if (_currentQueueName != null && _currentMessageHandler != null)
            {
                await SetupConsumerAsync(_currentQueueName, _currentMessageHandler, cancellationToken);
            }
        }
        finally
        {
            _channelLock.Release();
        }
    }

    private async Task SetupConsumerAsync(string queueName,
        Func<byte[], string, IDictionary<string, object>, Task<bool>> messageHandler,
        CancellationToken cancellationToken)
    {
        if (_channel == null) return;

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += async (sender, args) =>
        {
            var success = await HandleMessageAsync(args, messageHandler);
            await HandleAcknowledgmentAsync(args.DeliveryTag, success, cancellationToken);
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: _consumer,
            cancellationToken: cancellationToken);
    }

    private async Task<bool> HandleMessageAsync(
        BasicDeliverEventArgs args,
        Func<byte[], string, IDictionary<string, object>, Task<bool>> messageHandler)
    {
        try
        {
            return await messageHandler(
                args.Body.ToArray(),
                args.RoutingKey,
                args.BasicProperties.Headers ?? new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in message handler for routing key: {RoutingKey}", args.RoutingKey);
            return false;
        }
    }

    private async Task HandleAcknowledgmentAsync(ulong deliveryTag, bool success, CancellationToken cancellationToken)
    {
        if (_channel is not { IsOpen: true })
        {
            logger.LogWarning("Cannot acknowledge message - channel is not available");
            return;
        }

        try
        {
            if (success)
            {
                await _channel.BasicAckAsync(deliveryTag, false, cancellationToken);
                logger.LogDebug("Acknowledged message with delivery tag: {DeliveryTag}", deliveryTag);
            }
            else
            {
                await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                logger.LogDebug("Nacked message with delivery tag: {DeliveryTag}", deliveryTag);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to acknowledge message with delivery tag: {DeliveryTag}", deliveryTag);
        }
    }

    public async Task StopConsumingAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is { IsOpen: true })
        {
            try
            {
                await _channel.CloseAsync(cancellationToken: cancellationToken);
                logger.LogInformation("Stopped consuming from queue: {QueueName}", _currentQueueName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error stopping consumer for queue: {QueueName}", _currentQueueName);
            }
        }
        _currentQueueName = null;
        _currentMessageHandler = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await StopConsumingAsync();

        if (_channel != null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        _channelLock.Dispose();
        _disposed = true;
        logger.LogInformation("RabbitMQ consumer disposed");
    }
}
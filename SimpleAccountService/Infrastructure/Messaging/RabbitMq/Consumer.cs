using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Simple_Account_Service.Infrastructure.Messaging.RabbitMq;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "rabbitmq";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "account.crm";
    public string ExchangeName { get; set; } = "account.events";
}

public class EventConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<EventConsumer> logger,
    IOptions<RabbitMqOptions> options)
    : IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Объявление exchange и очереди, привязка (binding)
            await _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true);

            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue: _options.QueueName,
                exchange: _options.ExchangeName,
                routingKey: "account.#"); // Подписка на все события с префиксом account.

            logger.LogInformation("RabbitMQ initialized: Exchange {Exchange}, Queue {Queue} bound with routing key 'account.#'",
                _options.ExchangeName, _options.QueueName);
        }
        catch (BrokerUnreachableException ex)
        {
            logger.LogError(ex, "RabbitMQ broker unreachable");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during RabbitMQ initialization");
            throw;
        }
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        if (_channel == null)
            throw new InvalidOperationException("Channel not initialized. Call InitializeAsync first.");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                logger.LogInformation("Received message with routing key {RoutingKey}", ea.RoutingKey);

                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IEventHandler>();
                // IEventHandler – ваш интерфейс обработчика, нужно реализовать отдельно

                await handler.HandleAsync(messageJson, cancellationToken);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                logger.LogInformation("Message processed and acked.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
            }
        };

        _channel.BasicConsumeAsync(_options.QueueName, false, consumer, cancellationToken: cancellationToken);

        logger.LogInformation("Started consuming queue {Queue}", _options.QueueName);

        // Отдельный контролируемый цикл ожидания, если нужно
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(500, cancellationToken);
            }

            await DisposeAsync();
        }, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is { IsOpen: true })
            await _channel.CloseAsync();

        if (_connection is { IsOpen: true })
            await _connection.CloseAsync();

        _channel?.Dispose();
        _connection?.Dispose();

        logger.LogInformation("Consumer disposed");
    }
}

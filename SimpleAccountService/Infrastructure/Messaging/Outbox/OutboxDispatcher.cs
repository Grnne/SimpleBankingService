using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Simple_Account_Service.Application.Interfaces;

namespace Simple_Account_Service.Infrastructure.Messaging.Outbox;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "rabbitmq";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "account.events";
}

public class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxDispatcher> logger,
    IOptions<RabbitMqOptions> options)
    : IAsyncDisposable, IOutboxDispatcher, IDisposable
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

            await _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true);

            logger.LogInformation("RabbitMQ exchange declared: {Exchange}", _options.ExchangeName);
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

    public async Task DispatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ channel not initialized. Call InitializeAsync first.");
        }

        var messagesEnumerable = await outboxRepository.GetUnprocessedAsync(cancellationToken);

        var messages = (messagesEnumerable ?? []).ToList();

        if (messages.Count == 0)
        {
            logger.LogInformation("No outbox messages to dispatch");
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message.Payload);

                var props = new BasicProperties
                {
                    Persistent = true,
                    Headers = new Dictionary<string, object?>
                    {
                        { "x-correlation-id", message.CorrelationId.ToString() },
                        { "x-causation-id", message.CausationId.ToString() },
                        { "x-source", message.Source },
                        { "x-version", message.Version }
                    }
                };

                await _channel.BasicPublishAsync(
                    exchange: _options.ExchangeName,
                    routingKey: ToSnakeDotCase(message.EventType),
                    mandatory: true,
                    basicProperties: props,
                    body: body, cancellationToken: cancellationToken);

                await outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);

                logger.LogInformation("Dispatched message {MessageId} with routing key {RoutingKey}",
                    message.Id, ToSnakeDotCase(message.EventType));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dispatch message {MessageId}", message.Id);
            }
        }
    }

    private static string ToSnakeDotCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var stringBuilder = new StringBuilder();

        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                if (i > 0)
                {
                    stringBuilder.Append('.');
                }

                stringBuilder.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                stringBuilder.Append(input[i]);
            }
        }
        return stringBuilder.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null) await _connection.DisposeAsync();
        if (_channel != null) await _channel.DisposeAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _channel?.Dispose();
    }
}
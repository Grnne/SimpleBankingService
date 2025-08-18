using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Infrastructure.Messaging.Inbox;
using Simple_Account_Service.Infrastructure.Messaging.Outbox;
using System.Text;
using System.Text.Json;

namespace Simple_Account_Service.Features.Accounts.Consumers;

public class AntifraudConsumer(IServiceScopeFactory scopeFactory, ILogger<AntifraudConsumer> logger,
    IOptions<RabbitMqOptions> options) : BackgroundService, IAsyncDisposable
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
            await _channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, durable: true);
            await _channel.BasicQosAsync(0, 1, false);
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeAsync();

        if (_channel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
                var deadLetterRepository = scope.ServiceProvider.GetRequiredService<IInboxDeadLettersRepository>();
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                try
                {
                    object? inboxEvent = routingKey switch
                    {
                        "client.blocked" => JsonSerializer.Deserialize<ClientBlocked>(body, options),
                        "client.unblocked" => JsonSerializer.Deserialize<ClientUnblocked>(body, options),
                        _ => null
                    };

                    if (inboxEvent == null)
                    {
                        logger.LogWarning("Unsupported or failed to deserialize event for routing key {RoutingKey}", routingKey);
                        await AddToDeadLetterAsync(Guid.Empty, "AntifraudConsumer", body, "Invalid message format", deadLetterRepository, stoppingToken);
                        await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                        return;
                    }

                    await mediator.Publish(inboxEvent, stoppingToken);
                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                }
                catch (JsonException jsonEx)
                {
                    logger.LogWarning(jsonEx, "JSON deserialization error for routing key {RoutingKey}", routingKey);
                    await AddToDeadLetterAsync(Guid.Empty, "AntifraudConsumer", body, $"Deserialization error: {jsonEx.Message}", deadLetterRepository, stoppingToken);
                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                }
                catch (NotSupportedException notSupEx)
                {
                    logger.LogWarning(notSupEx, "Unsupported JSON content for routing key {RoutingKey}", routingKey);
                    await AddToDeadLetterAsync(Guid.Empty, "AntifraudConsumer", body, $"Unsupported content: {notSupEx.Message}", deadLetterRepository, stoppingToken);
                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error processing message for routing key {RoutingKey}", routingKey);
                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(queue: "account.antifraud", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        }

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null) await _connection.DisposeAsync();
        if (_channel != null) await _channel.DisposeAsync();
    }

    private static async Task AddToDeadLetterAsync(
        Guid eventId,
        string handler,
        string payload,
        string error,
        IInboxDeadLettersRepository deadLetterRepository,
        CancellationToken cancellationToken)
    {
        var deadLetter = new InboxDeadLetter
        {
            MessageId = eventId == Guid.Empty ? Guid.NewGuid() : eventId,
            Handler = handler,
            Payload = payload,
            Error = error,
            ReceivedAt = DateTime.UtcNow
        };
        await deadLetterRepository.AddAsync(deadLetter, cancellationToken);
    }
}
using System.Text;
using System.Text.Json;
using MediatR;
using RabbitMQ.Client.Events;
using Simple_Account_Service.Application.Interfaces.Repositories;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Infrastructure.Messaging.Inbox;
using Simple_Account_Service.Infrastructure.Messaging.RabbitMq;

namespace Simple_Account_Service.Application.Services.Messaging;

//public class AntifraudService(ILogger<AntifraudService> logger, RabbitMqConnectionFactory factory) : BackgroundService, IAsyncDisposable
//{
    //private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {

//        if (_channel != null)
//        {
//            var consumer = new AsyncEventingBasicConsumer(_channel);

//            consumer.ReceivedAsync += async (sender, ea) =>
//            {
//                using var scope = scopeFactory.CreateScope();
//                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
//                scope.ServiceProvider.GetRequiredService<IInboxRepository>();
//                var deadLetterRepository = scope.ServiceProvider.GetRequiredService<IInboxDeadLettersRepository>();
//                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
//                var routingKey = ea.RoutingKey;
//                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

//                try
//                {
//                    object? inboxEvent = routingKey switch
//                    {
//                        "client.blocked" => JsonSerializer.Deserialize<ClientBlocked>(body, options),
//                        "client.unblocked" => JsonSerializer.Deserialize<ClientUnblocked>(body, options),
//                        _ => null
//                    };

//                    if (inboxEvent == null)
//                    {
//                        logger.LogWarning("Unsupported or failed to deserialize event for routing key {RoutingKey}", routingKey);
//                        await AddToDeadLetterAsync(Guid.Empty, "AntifraudConsumer", body, "Invalid message format", deadLetterRepository, stoppingToken);
//                        await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
//                        return;
//                    }

//                    await mediator.Publish(inboxEvent, stoppingToken);
//                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
//                }
//                catch (JsonException jsonEx)
//                {
//                    logger.LogWarning(jsonEx, "JSON deserialization error for routing key {RoutingKey}", routingKey);
//                    await AddToDeadLetterAsync(Guid.Empty, "AntifraudConsumer", body, $"Deserialization error: {jsonEx.Message}", deadLetterRepository, stoppingToken);
//                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
//                }
//                catch (NotSupportedException notSupEx)
//                {
//                    logger.LogWarning(notSupEx, "Unsupported JSON content for routing key {RoutingKey}", routingKey);
//                    await AddToDeadLetterAsync(Guid.Empty, "AntifraudConsumer", body, $"Unsupported content: {notSupEx.Message}", deadLetterRepository, stoppingToken);
//                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Unexpected error processing message for routing key {RoutingKey}", routingKey);
//                    await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, cancellationToken: stoppingToken);
//                }
//            };

//            await _channel.BasicConsumeAsync(queue: "account.antifraud", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
//        }

//        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
//    }

//    public async ValueTask DisposeAsync()
//    {
//        if (_connection != null) await _connection.DisposeAsync();
//        if (_channel != null) await _channel.DisposeAsync();
//    }

//    private static async Task AddToDeadLetterAsync(
//        Guid eventId,
//        string handler,
//        string payload,
//        string error,
//        IInboxDeadLettersRepository deadLetterRepository,
//        CancellationToken cancellationToken)
//    {
//        var deadLetter = new InboxDeadLetter
//        {
//            MessageId = eventId == Guid.Empty ? Guid.NewGuid() : eventId,
//            Handler = handler,
//            Payload = payload,
//            Error = error,
//            ReceivedAt = DateTime.UtcNow
//        };
//        await deadLetterRepository.AddAsync(deadLetter, cancellationToken);
//    }
//}
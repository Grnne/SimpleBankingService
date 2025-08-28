using RabbitMQ.Client;
using System.Text;
using Simple_Account_Service.Application.Interfaces.Messaging;

namespace Simple_Account_Service.Infrastructure.Messaging.RabbitMq;

public class RabbitMqPublisher(RabbitMqConnectionFactory factory, ILogger<RabbitMqPublisher> logger) : IRabbitMqPublisher
{
    public IChannel Channel { get; private set; } = null!;

    public async Task InitAsync()
    {
        Channel = await factory.GetChannelAsync();
    }

    public async Task<List<(Guid, DateTime)>> PublishMessagesReturnPublishedIds(List<OutboxToRabbitMqDto> messages, CancellationToken cancellationToken)
    {
        var publishedIds = new List<(Guid, DateTime)>();

        foreach (var message in messages)
        {
            try
            {
                await Channel.BasicPublishAsync(
                    exchange: factory.ExchangeName,
                    routingKey: GetRoutingKey(message.EventType),
                    mandatory: true,
                    basicProperties: message.Props,
                    body: message.Body,
                    cancellationToken: cancellationToken);

                publishedIds.Add((message.Id, DateTime.UtcNow));

                logger.LogInformation(
                    "Message {MessageId} Correlation id {CorrelationId} Causation id {CausationId} published and confirmed",
                    message.Id, message.CorrelationId, message.CausationId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Message {MessageId} Correlation id {CorrelationId} Causation id {CausationId} failed to publish",
                    message.Id, message.CorrelationId, message.CausationId);
            }
        }

        return publishedIds;
    }

    private static string GetRoutingKey(string input)
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
}
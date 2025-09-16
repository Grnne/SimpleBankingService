namespace Simple_Account_Service.Application.Interfaces.Messaging;

public interface IRabbitMqConsumer : IAsyncDisposable
{
    Task StartConsumingAsync(
        string queueName,
        Func<byte[], string, IDictionary<string, object>, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default);

    Task StopConsumingAsync(CancellationToken cancellationToken = default);
}
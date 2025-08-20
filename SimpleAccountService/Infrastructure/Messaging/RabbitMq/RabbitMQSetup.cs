using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Simple_Account_Service.Infrastructure.Messaging.RabbitMq;

public class RabbitMqSetup(ILogger<RabbitMqSetup> logger, IConfiguration configuration) : IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _hostName = configuration["RabbitMQ:Host"] ?? "localhost";
    private readonly int _port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672;
    private readonly string _userName = configuration["RabbitMQ:UserName"] ?? "guest";
    private readonly string _password = configuration["RabbitMQ:Password"] ?? "guest";


    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password,
            AutomaticRecoveryEnabled = true
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: "account.events",
            type: ExchangeType.Topic,
            durable: true);
        try
        {
            await _channel.QueueDeclareAsync("account.crm", true, false, false);
            await _channel.QueueBindAsync("account.crm", "account.events", "account.#");

            await _channel.QueueDeclareAsync("account.notifications", true, false, false);
            await _channel.QueueBindAsync("account.notifications", "account.events", "money.*");

            await _channel.QueueDeclareAsync("account.antifraud", true, false, false);
            await _channel.QueueBindAsync("account.antifraud", "account.events", "client.*");

            await _channel.QueueDeclareAsync("account.audit", true, false, false);
            await _channel.QueueBindAsync("account.audit", "account.events", "#");

            logger.LogInformation("RabbitMQ topology initialized: exchange and queues declared with bindings.");
        }
        catch (BrokerUnreachableException ex)
        {
            logger.LogError(ex, "Could not reach RabbitMQ broker");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during RabbitMQ setup");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null && _channel.IsOpen)
        {
            await _channel.CloseAsync();
        }

        if (_connection is not null && _connection.IsOpen)
        {
            await _connection.CloseAsync();
        }

        await Task.CompletedTask;
    }
}
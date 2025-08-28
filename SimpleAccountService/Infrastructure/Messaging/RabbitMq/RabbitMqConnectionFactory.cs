using RabbitMQ.Client;

namespace Simple_Account_Service.Infrastructure.Messaging.RabbitMq;


public class RabbitMqConnectionFactory : IAsyncDisposable
{
    private IConnection? _connection;
    public string ExchangeName { get; set; } = "account.events";
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private readonly Task _initializationTask;

    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;

    public RabbitMqConnectionFactory(ILogger<RabbitMqConnectionFactory> logger, IConfiguration configuration)
    {
        _logger = logger;

        _connectionFactory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _initializationTask = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await GetConnectionAsync();
            _logger.LogInformation("RabbitMQ connection factory initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _connectionLock.WaitAsync();

        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _logger.LogInformation("Creating new RabbitMQ connection");
            _connection = await _connectionFactory.CreateConnectionAsync();

            await using var channel = await _connection.CreateChannelAsync(new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            ));

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true);

            await channel.QueueDeclareAsync("account.crm", true, false, false);
            await channel.QueueBindAsync("account.crm", "account.events", "account.#");

            await channel.QueueDeclareAsync("account.notifications", true, false, false);
            await channel.QueueBindAsync("account.notifications", "account.events", "money.*");

            await channel.QueueDeclareAsync("account.antifraud", true, false, false);
            await channel.QueueBindAsync("account.antifraud", "account.events", "client.*");

            await channel.QueueDeclareAsync("account.audit", true, false, false);
            await channel.QueueBindAsync("account.audit", "account.events", "#");

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<IChannel> GetChannelAsync()
    {
        if (!_initializationTask.IsCompleted)
        {
            await _initializationTask;
        }

        var connection = await GetConnectionAsync();

        _logger.LogInformation("Creating new RabbitMqChannel");

        var channel = await connection.CreateChannelAsync(new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
                ));

        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
        }
        _connectionLock.Dispose();
    }
}
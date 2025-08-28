using DotNet.Testcontainers.Builders;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Simple_Account_Service;
using Simple_Account_Service.Extensions;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Messaging.RabbitMq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SimpleAccountService.Tests.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("SASDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("pg_isready -U postgres"))
        .Build();
    private readonly RabbitMqContainer _rmqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1.3-management")
        .WithUsername("guest")
        .WithPassword("guest")

        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rmqContainer.StartAsync();

        await Task.Delay(5000);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var testConnectionString = _postgresContainer.GetConnectionString();
        const string rabbitHostName = "localhost"; // Для TCP подключения снаружи контейнера
        var rabbitPort = _rmqContainer.GetMappedPublicPort(5672);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var dict = new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = testConnectionString,
                ["RabbitMQ:Host"] = rabbitHostName,
                ["RabbitMQ:Port"] = rabbitPort.ToString(),
                ["RabbitMQ:Username"] = "guest",
                ["RabbitMQ:Password"] = "guest"
            };
            config.AddInMemoryCollection(dict.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)));
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll<DbContextOptions<SasDbContext>>();
            services.AddDbContext<SasDbContext>(options =>
                options.UseNpgsql(testConnectionString));

            services.RemoveAll<IConnection>();
            services.RemoveAll<IConnectionFactory>();
            services.RemoveAll<RabbitMqConnectionFactory>();


            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
            services.AddAuthorization();
        });
    }
    [UsedImplicitly]
    public async Task StopRabbitMqAsync() // TODO 
    {
        await _rmqContainer.StopAsync();
    }
    [UsedImplicitly]
    public async Task StartRabbitMqAsync()
    {
        await _rmqContainer.StartAsync();
        await Task.Delay(10000);
    }

    public new async ValueTask DisposeAsync()
    {
        await _rmqContainer.StopAsync();
        await _postgresContainer.StopAsync();
        await _rmqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
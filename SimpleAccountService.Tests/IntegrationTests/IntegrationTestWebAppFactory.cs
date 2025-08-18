using DotNet.Testcontainers.Builders;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Simple_Account_Service;
using Simple_Account_Service.Infrastructure.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SimpleAccountService.Tests.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rmqContainer;

    public IntegrationTestWebAppFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("SASDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithPortBinding(5435, 5432)
            .Build();

        _rmqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.1.3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithPortBinding(5673, 5672)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                ["RabbitMQ:HostName"] = "localhost", 
                ["RabbitMQ:UserName"] = "guest",
                ["RabbitMQ:Password"] = "guest"
            };

            config.AddInMemoryCollection(settings!);
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SasDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<SasDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            var rabbitDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnection));
            if (rabbitDescriptor != null)
            {
                services.Remove(rabbitDescriptor);
            }

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            var rabbitConnection = factory.CreateConnectionAsync();
            
            services.AddSingleton<IConnection>(sp =>
            {
                var rabbitFactory = sp.GetRequiredService<ConnectionFactory>();
                return rabbitFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            services.AddAuthorization();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rmqContainer.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await _rmqContainer.StopAsync();
        await _postgresContainer.StopAsync();
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

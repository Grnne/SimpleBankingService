using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Simple_Account_Service;
using Simple_Account_Service.Infrastructure.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;
using JetBrains.Annotations;
using Testcontainers.PostgreSql;

namespace SimpleAccountService.Tests.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("SASDb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithImage("postgres:latest")
        .WithPortBinding(5435, 5432)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SasDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<SasDbContext>(options => options.UseNpgsql(_postgresContainer.GetConnectionString()));
            
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", ([UsedImplicitly] options) => { });

            services.AddAuthorization();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
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
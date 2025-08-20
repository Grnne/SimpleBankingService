using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using RabbitMQ.Client;
using Simple_Account_Service.Application.Behaviors;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Features.Accounts;
using Simple_Account_Service.Features.Accounts.Consumers;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Messaging.Outbox;
using Simple_Account_Service.Infrastructure.Messaging.RabbitMq;
using Simple_Account_Service.Infrastructure.Middleware;
using Simple_Account_Service.Infrastructure.Repositories;
using System.Reflection;
using IConnectionFactory = RabbitMQ.Client.IConnectionFactory;

namespace Simple_Account_Service.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<ITransactionService, TransactionsService>();
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IInboxRepository, InboxRepository>();
        services.AddScoped<IInboxDeadLettersRepository, InboxDeadLettersRepository>();
        services.AddHostedService<OutboxDispatcher>();
        services.AddHostedService<AntifraudConsumer>();

        return services;
    }

    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", b =>
            {
                b.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        // For dummy Keycloak token request
        services.AddHttpClient();

        return services;
    }

    public static IServiceCollection AddCustomRabbitMq(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? env = null)
    {
        if (env != null && env.IsEnvironment("IntegrationTests"))
            return services; // Пропускаем регистрацию в тестах

        services.AddSingleton<IConnectionFactory>(_ =>
            new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                Port = configuration["RabbitMQ:Port"] != null ? int.Parse(configuration["RabbitMQ:Port"]!) : AmqpTcpEndpoint.UseDefaultPort,
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                AutomaticRecoveryEnabled = true
            });

        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IConnectionFactory>();
            return factory.CreateConnectionAsync();
        });

        services.AddSingleton<RabbitMqSetup>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqSetup>>();
            var config = sp.GetRequiredService<IConfiguration>();
            return new RabbitMqSetup(logger, config);
        });

        return services;
    }


    public static IServiceCollection AddCustomMediatr(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }

    public static IServiceCollection AddCustomFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }

    public static IServiceCollection AddCustomHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(configuration
                    .GetConnectionString("DefaultConnection"))));
        services.AddHangfireServer();

        return services;
    }


}
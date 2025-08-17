using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Simple_Account_Service.Application.Behaviors;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Features.Accounts;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Messaging.RabbitMq;
using Simple_Account_Service.Infrastructure.Middleware;
using Simple_Account_Service.Infrastructure.Repositories;
using System.Reflection;

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
        
        services.AddSingleton<RabbitMqSetup>();
        // For dummy Keycloak token request
        services.AddHttpClient();

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
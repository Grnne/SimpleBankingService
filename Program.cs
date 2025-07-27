using FluentValidation;
using Microsoft.OpenApi.Models;
using Simple_Account_Service.Application.Behaviors;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Features.Accounts;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Middleware;
using Simple_Account_Service.Infrastructure.Repositories;
using System.Reflection;

namespace Simple_Account_Service;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddSingleton<FakeDb>();

        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();

        builder.Services.AddScoped<IAccountsService, AccountsService>(); //TODO interfaces
        builder.Services.AddScoped<ITransactionService, TransactionsService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Simple Account Service API", Version = "v1" });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        var app = builder.Build();

        //Refactor for build\dev
        //app.UseDeveloperExceptionPage(); 
        app.UseExceptionHandler();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Account Service API V1");
            c.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
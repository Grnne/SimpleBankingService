using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

//TODO wrap responses

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        // TODO после кейклоки
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://keycloak:8080/realms/my_realm"; 
                options.Audience = "my_client"; 
                options.RequireHttpsMetadata = false; 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", b =>
            {
                b.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

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

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }});
        });

        //For dummy keycloak token request
        builder.Services.AddHttpClient(); 


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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.UseCors("AllowAll");

        app.Run();
    }
}
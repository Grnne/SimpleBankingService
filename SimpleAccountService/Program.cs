using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Simple_Account_Service.Application.Behaviors;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Middleware;
using Simple_Account_Service.Infrastructure.Repositories;
using System.Net;
using System.Reflection;

namespace Simple_Account_Service;

// TODO https certificates self-signed or let's encrypt, some more tests; read bout uuid7, ask questions below

// Спросить про: NodaTime, транзакции, выписки(проекции и поле для промежуточного ежемесячного баланса
// возможно), 2 запроса или обработка большего количества данных, про полотно в program или отдельные файлы
// с конфигурацией, про concurrency в контексте банковских операций, про возврат 204 delete
// вспомнить, что хотел спросить(критически важно 🤡)

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

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

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var mbError = new MbError(HttpStatusCode.Unauthorized, "Unauthorized", "Authentication is required or token is invalid.");
                        var result = new MbResult<object>(mbError);

                        await context.Response.WriteAsJsonAsync(result);
                    }
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
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
        builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
        builder.Services.AddScoped<ITransactionService, TransactionsService>();
        builder.Services.AddScoped<IAccountsService, AccountsService>();

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

        builder.Services.AddDbContext<SasDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
                o =>
                {
                    o.MapEnum<TransactionType>();
                    o.MapEnum<AccountType>();
                }
            ));

        // For dummy Keycloak token request
        builder.Services.AddHttpClient();

        builder.Services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(builder.Configuration
                    .GetConnectionString("DefaultConnection"))));
        builder.Services.AddHangfireServer();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();
            var fakeDb = scope.ServiceProvider.GetRequiredService<FakeDb>();

            // TODO don't forget to ask questions
            // I see in output during migration and deletion Exception thrown: 'System.Net.Sockets.SocketException' in System.Net.Sockets.dll
            // It does not affect application operation, but I cannot catch it

            try
            {
                Console.WriteLine("init migration");
                // For dev purposes
                //context.Database.EnsureDeleted(); 
                context.Database.Migrate();
            }
            catch (Exception e)
            {
                Console.WriteLine("wtf" + e.Message);
                throw;
            }

            DataSeeder.SeedFakeData(context, fakeDb);
        }

        // Refactor for build/dev
        //app.UseDeveloperExceptionPage();
        app.UseExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Account Service API V1");
            c.RoutePrefix = "swagger";
        });

        // app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", context =>
        {
            context.Response.Redirect("/swagger");
            return Task.CompletedTask;
        });

        app.UseCors("AllowAll");

        app.MapControllers();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new List<IDashboardAuthorizationFilter>
            {
                new AllowAllDashboardAuthorizationFilter()
            }
        });

        RecurringJob.AddOrUpdate<IAccountsService>(
            "DailyInterestAccrualJob",
            service => service.AddDailyInterestAsync(),
            Cron.Daily);

        app.Run();
    }
}

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
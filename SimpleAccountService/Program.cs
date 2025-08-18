using Hangfire;
using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Extensions;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Infrastructure.Data;
using System.Threading;

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

        builder.Services
            .AddApplicationServices()
            .AddCustomDbContexts(builder.Configuration)
            .AddCustomAuthentication()
            .AddCustomMediatr()
            .AddCustomFluentValidation()
            .AddCommonServices()
            .AddAutoMapper(typeof(Program))
            .AddCustomSwagger()
            .AddCustomHangfire(builder.Configuration)
            .AddMassTransitWithRabbitMq(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SasDbContext>();
            var fakeDb = scope.ServiceProvider.GetRequiredService<FakeDb>();

            try
            {
                Console.WriteLine("init migration");
                // For dev purposes
                context.Database.EnsureDeleted();
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
        app.UseDeveloperExceptionPage();
        //app.UseExceptionHandler();

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
            service => service.AddDailyInterestAsync(CancellationToken.None),
            Cron.Daily);

        RecurringJob.AddOrUpdate<IOutboxDispatcher>(
            "OutboxDispatcherJob",
            dispatcher => dispatcher.DispatchAsync(CancellationToken.None),
            Cron.Minutely);

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
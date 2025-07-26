using FluentValidation;
using Microsoft.OpenApi.Models;
using Simple_Account_Service.Application.Behaviors;
using Simple_Account_Service.Infrastructure.FakeDb;
using Simple_Account_Service.Infrastructure.Middleware;
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

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Simple Account Service API", Version = "v1" });
            // Мб добавить xml потом
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple Account Service API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddCustomDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<FakeDb>();

        services.AddDbContext<SasDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
                o =>
                {
                    o.MapEnum<TransactionType>();
                    o.MapEnum<AccountType>();
                }
            ));

        return services;
    }
}
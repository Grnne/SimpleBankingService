using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Features.Accounts;

public class AccountsService(IAccountRepository repository, SasDbContext context, ILogger<AccountsService> logger) : IAccountsService
{
    public async Task AddDailyInterestAsync()
    {
        var accounts = await repository.GetAllAccountsAsync();

        foreach (var account in accounts)
        {
            try
            {
                await context.Database.ExecuteSqlInterpolatedAsync($"CALL public.accrue_interest({account.Id})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to accrue interest for account {AccountId}", account.Id);
            }
        }

        logger.LogInformation("Interest accrued");
    }
}
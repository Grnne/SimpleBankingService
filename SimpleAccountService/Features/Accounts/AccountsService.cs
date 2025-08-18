using MediatR;
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Features.Accounts;

public class AccountsService(IAccountRepository repository, SasDbContext context, ILogger<AccountsService> logger, IMediator mediator) : IAccountsService
{
    public async Task AddDailyInterestAsync()
    {
        var accounts = await repository.GetAllAccountsAsync();

        foreach (var account in accounts)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var periodFrom = account.LastInterestAccrualAt ?? DateTime.UtcNow.AddDays(-1);

                await context.Database.ExecuteSqlInterpolatedAsync($"CALL public.accrue_interest({account.Id})");

                var updatedAccount = await context.Accounts
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(a => a.Id == account.Id)
                                     ?? throw new InvalidOperationException($"Account {account.Id} not found after interest accrual");

                var periodTo = updatedAccount.LastInterestAccrualAt ?? DateTime.UtcNow;

                var amount = updatedAccount.Balance - account.Balance;

                var interestAccrued = new AccountInterestAccrued(
                    AccountId: account.Id,
                    PeriodFrom: periodFrom,
                    PeriodTo: periodTo,
                    Amount: amount,
                    Source: "accounts",
                    CorrelationId: Guid.NewGuid(),
                    CausationId: Guid.NewGuid()
                );

                await mediator.Publish(interestAccrued);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to accrue interest for account {AccountId}", account.Id);
            }
        }

        logger.LogInformation("Interest accrued");
    }
}
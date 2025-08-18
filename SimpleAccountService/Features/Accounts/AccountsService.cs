using MassTransit;
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Features.Accounts.Interfaces;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Features.Accounts;

public class AccountsService(IAccountRepository repository, SasDbContext context,
    ILogger<AccountsService> logger, IPublishEndpoint publishEndpoint) : IAccountsService
{
    public async Task AddDailyInterestAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await repository.GetAllAccountsAsync(cancellationToken);
        foreach (var account in accounts)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var periodFrom = account.LastInterestAccrualAt ?? DateTime.UtcNow.AddDays(-1);

                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"CALL public.accrue_interest({account.Id})", cancellationToken);

                var updatedAccount = await context.Accounts
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(a => a.Id == account.Id, cancellationToken)
                                     ?? throw new InvalidOperationException($"Account {account.Id} not found after interest accrual");

                var periodTo = updatedAccount.LastInterestAccrualAt ?? DateTime.UtcNow;
                var amount = updatedAccount.Balance - account.Balance;

                var interestAccruedEvent = new InterestAccrued(
                    AccountId: account.Id,
                    PeriodFrom: periodFrom,
                    PeriodTo: periodTo,
                    Amount: amount,
                    Source: "accounts",
                    CorrelationId: Guid.NewGuid(),
                    CausationId: Guid.NewGuid());

                await publishEndpoint.Publish(interestAccruedEvent, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger.LogError(ex, "Failed to accrue interest for account {AccountId}", account.Id);
            }
        }

        logger.LogInformation("Interest accrued successfully for all accounts");
    }
}
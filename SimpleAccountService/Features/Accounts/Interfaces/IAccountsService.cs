namespace Simple_Account_Service.Features.Accounts.Interfaces;

public interface IAccountsService
{
    Task AddDailyInterestAsync(CancellationToken cancellationToken);
}
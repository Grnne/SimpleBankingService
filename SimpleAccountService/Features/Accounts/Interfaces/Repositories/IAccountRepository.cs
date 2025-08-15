using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Features.Accounts.Entities;

namespace Simple_Account_Service.Features.Accounts.Interfaces.Repositories;

public interface IAccountRepository : IBaseRepository<Account>
{
    public Task<IEnumerable<Account>> GetAllAccountsAsync();

    Task<IEnumerable<Account>> GetAccountsEagerlyUpToEndDateByOwnerAsync(Guid ownerId, DateTime endDate);
}
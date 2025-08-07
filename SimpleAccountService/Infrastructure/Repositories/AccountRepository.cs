using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class AccountRepository(FakeDb db) : IAccountRepository
{
    public async Task<Account> CreateAsync(Account entity)
    {
        await db.AddAccountAsync(entity);

        return entity;
    }

    public async Task<Account?> GetByIdAsync(Guid accountId)
    {
        var existing = await db.GetAccountByIdAsync(accountId);

        return existing;
    }

    public async Task<Account> UpdateAsync(Account entity)
    {
        var updated = await db.UpdateAccountAsync(entity);

        if (!updated)
        {
            throw new InvalidOperationException("Ошибка обновления счета на уровне базы данных. О как!");
        }

        return entity;
    }

    public async Task<bool> DeleteAsync(Guid accountId)
    {
        return await db.RemoveAccountAsync(accountId);
    }

    public async Task<IEnumerable<Account>> GetAllAccountsAsync()
    {
        return await db.GetAllAccountsAsync();
    }
}
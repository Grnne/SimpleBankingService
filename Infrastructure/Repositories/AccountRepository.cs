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

    public async Task DeleteAsync(Guid accountId)
    {
        var deleted = await db.RemoveAccountAsync(accountId);

        if (!deleted)
        {
            throw new InvalidOperationException("Ошибка удаления счета на уровне базы данных. О как!");
        }
    }

    public async Task<IEnumerable<Account>> GetAllAccountsAsync()
    {
        return await db.GetAllAccountsAsync();
    }
}
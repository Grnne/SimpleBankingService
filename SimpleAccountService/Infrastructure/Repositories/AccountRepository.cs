using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Features.Accounts.Entities;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class AccountRepository(SasDbContext context) : IAccountRepository
{
    public async Task<Account> CreateAsync(Account entity)
    {
        await context.Accounts.AddAsync(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    public async Task<Account?> GetByIdAsync(Guid accountId)
    {
        return await context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == accountId);
    }

    public async Task<Account> UpdateAsync(Account entity)
    {
        context.Accounts.Update(entity);

        var result = await context.SaveChangesAsync();

        if (result == 0)
        {
            throw new InvalidOperationException("Ошибка обновления счета на уровне базы данных. О как!");
        }

        return entity;
    }

    public async Task<bool> DeleteAsync(Guid accountId)
    {
        var entity = await context.Accounts.FindAsync(accountId);

        if (entity == null)
        {
            return false;
        }

        context.Accounts.Remove(entity);

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Account>> GetAllAccountsAsync(CancellationToken cancellationToken)
    {
        return await context.Accounts
            .Include(a => a.Transactions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetAccountsEagerlyUpToEndDateByOwnerAsync(Guid ownerId, DateTime endDate)
    {
        return await context.Accounts
            .Where(a => a.OwnerId == ownerId && a.CreatedAt <= endDate)
            .Include(a => a.Transactions.Where(t => t.Timestamp <= endDate))
            .ToListAsync();
    }
}
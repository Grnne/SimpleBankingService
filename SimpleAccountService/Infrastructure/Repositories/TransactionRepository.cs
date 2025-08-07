using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class TransactionRepository(SasDbContext context) : ITransactionRepository

{
    public async Task<Transaction?> GetByIdAsync(Guid accountId)
    {
        return await context.Transactions.FindAsync(accountId);
    }

    public async Task<Transaction> CreateAsync(Transaction entity)
    {
         await context.Transactions.AddAsync(entity);
         await context.SaveChangesAsync();

         return entity;
    }

    public async Task<Transaction> UpdateAsync(Transaction entity)
    {
        context.Transactions.Update(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    public async Task<bool> DeleteAsync(Guid transactionId)
    {
        var entity = await context.Transactions.FindAsync(transactionId);
        
        if (entity == null)
        {
            return false;
        }

        context.Transactions.Remove(entity);
        await context.SaveChangesAsync();

        return true;
    }
}
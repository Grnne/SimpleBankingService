using Simple_Account_Service.Features.Transactions.Entities;
using Simple_Account_Service.Features.Transactions.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class TransactionRepository(FakeDb db) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid accountId)
    {
        return await db.GetTransactionByIdAsync(accountId);
    }

    public async Task<Transaction> CreateAsync(Transaction entity)
    {
        return await db.AddTransactionAsync(entity);
    }

    public async Task<Transaction> UpdateAsync(Transaction entity)
    {
        await db.UpdateTransactionAsync(entity);
        return entity;
    }

    public async Task DeleteAsync(Guid accountId)
    {
        await db.RemoveTransactionAsync(accountId);
    }
}
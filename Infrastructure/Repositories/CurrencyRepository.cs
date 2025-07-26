using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class CurrencyRepository(FakeDb db) : ICurrencyRepository
{
    public async Task<Currency?> GetByCodeAsync(string code)
    {
        return await db.GetCurrencyByCodeAsync(code);
    }

    public async Task<Currency> AddAsync(Currency entity)
    {
        return await db.AddCurrencyAsync(entity);
    }

    public async Task<Currency> UpdateAsync(Currency entity)
    {
        await db.UpdateCurrencyAsync(entity);
        return entity;
    }

    public async Task RemoveAsync(string code)
    {
        await db.RemoveCurrencyAsync(code);
    }
}
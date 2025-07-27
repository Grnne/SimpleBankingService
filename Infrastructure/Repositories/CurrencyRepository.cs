using JetBrains.Annotations;
using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

[UsedImplicitly]
public class CurrencyRepository(FakeDb db) : ICurrencyRepository
{
    [UsedImplicitly]
    public async Task<Currency?> GetByCodeAsync(string code)
    {
        return await db.GetCurrencyByCodeAsync(code);
    }

    [UsedImplicitly]
    public async Task<Currency> AddAsync(Currency entity)
    {
        return await db.AddCurrencyAsync(entity);
    }

    [UsedImplicitly]
    public async Task<Currency> UpdateAsync(Currency entity)
    {
        await db.UpdateCurrencyAsync(entity);
        return entity;
    }

    [UsedImplicitly]
    public async Task RemoveAsync(string code)
    {
        await db.RemoveCurrencyAsync(code);
    }
}
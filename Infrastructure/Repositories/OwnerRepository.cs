using Simple_Account_Service.Application.ForFakesAndDummies;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class OwnerRepository : IOwnerRepository
{
    private readonly FakeDb _db;

    public OwnerRepository(FakeDb db)
    {
        _db = db;
    }

    public async Task<Owner?> GetByIdAsync(Guid id)
    {
        return await _db.GetOwnerByIdAsync(id);
    }

    public async Task<Owner> AddAsync(Owner entity)
    {
        return await _db.AddOwnerAsync(entity);
    }

    public async Task<Owner> UpdateAsync(Owner entity)
    {
        await _db.UpdateOwnerAsync(entity);
        return entity;
    }

    public async Task RemoveAsync(Guid id)
    {
        await _db.RemoveOwnerAsync(id);
    }
}
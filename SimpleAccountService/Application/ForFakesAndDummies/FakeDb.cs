namespace Simple_Account_Service.Application.ForFakesAndDummies;

public class FakeDb
{
    private readonly List<Owner> _owners = [];

    public FakeDb()
    {
        InitializeFakeData();
    }

    public Task<IEnumerable<Owner>> GetAllOwnersAsync() =>
        Task.FromResult<IEnumerable<Owner>>(_owners);

    public Task<Owner?> GetOwnerByIdAsync(Guid id) =>
        Task.FromResult(_owners.FirstOrDefault(o => o.Id == id));

    public Task<Owner> AddOwnerAsync(Owner owner)
    {
        _owners.Add(owner);
        return Task.FromResult(owner);
    }

    public Task<bool> UpdateOwnerAsync(Owner updated)
    {
        var index = _owners.FindIndex(o => o.Id == updated.Id);
        if (index < 0) return Task.FromResult(false);

        _owners[index] = updated;
        return Task.FromResult(true);
    }

    public Task<bool> RemoveOwnerAsync(Guid id)
    {
        var owner = _owners.FirstOrDefault(o => o.Id == id);
        if (owner == null) return Task.FromResult(false);

        _owners.Remove(owner);
        return Task.FromResult(true);
    }

    private void InitializeFakeData()
    {
        _owners.AddRange(
        [   new Owner { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "test1" },
            new Owner { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "test2" },
            new Owner { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "test3" }
        ]);
    }
}
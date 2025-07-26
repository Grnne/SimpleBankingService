namespace Simple_Account_Service.Application.Interfaces;

public interface IBaseRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(Guid id);

    public Task<T> AddAsync(T entity);

    public Task<T> UpdateAsync(T entity);

    public Task RemoveAsync(Guid id);
}
﻿namespace Simple_Account_Service.Application.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(Guid id);

    public Task<T> CreateAsync(T entity);

    public Task<T> UpdateAsync(T entity);

    public Task<bool> DeleteAsync(Guid id);
}
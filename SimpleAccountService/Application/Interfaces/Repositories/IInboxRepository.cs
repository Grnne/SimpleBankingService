namespace Simple_Account_Service.Application.Interfaces.Repositories;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken);
    Task AddProcessedAsync(Guid eventId, string handler, CancellationToken cancellationToken);
}
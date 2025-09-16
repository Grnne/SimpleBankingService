using Simple_Account_Service.Infrastructure.Messaging.Inbox;

namespace Simple_Account_Service.Application.Interfaces.Repositories;

public interface IInboxDeadLettersRepository
{
    Task AddAsync(InboxDeadLetter deadLetter, CancellationToken cancellationToken);
}
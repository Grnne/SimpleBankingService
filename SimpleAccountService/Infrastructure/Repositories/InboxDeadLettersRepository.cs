using Simple_Account_Service.Application.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Data;
using Simple_Account_Service.Infrastructure.Messaging.Inbox;

namespace Simple_Account_Service.Infrastructure.Repositories;

public class InboxDeadLettersRepository(SasDbContext context) : IInboxDeadLettersRepository
{
    public async Task AddAsync(InboxDeadLetter deadLetter, CancellationToken cancellationToken)
    {
        await context.InboxDeadLetters.AddAsync(deadLetter, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
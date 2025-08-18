using System;
using System.Threading;
using System.Threading.Tasks;
using Simple_Account_Service.Infrastructure.Messaging.Inbox;

namespace Simple_Account_Service.Application.Interfaces;

public interface IInboxDeadLettersRepository
{
    Task AddAsync(InboxDeadLetter deadLetter, CancellationToken cancellationToken);
}
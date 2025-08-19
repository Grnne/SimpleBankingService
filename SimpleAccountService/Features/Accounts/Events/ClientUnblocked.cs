using JetBrains.Annotations;
using Simple_Account_Service.Application.Abstractions;
using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Features.Accounts.Interfaces.Repositories;
using Simple_Account_Service.Infrastructure.Messaging.Inbox;
using System.Text.Json;

namespace Simple_Account_Service.Features.Accounts.Events;

public record ClientUnblocked(Guid ClientId, Guid EventId, DateTime OccurredAt, EventMeta Meta)
    : AntifraudEvent(EventId, OccurredAt, Meta);

[UsedImplicitly]
public class ClientUnblockedHandler(IInboxRepository inboxRepository, IAccountRepository accountRepository,
    IInboxDeadLettersRepository deadRepository, ILogger<ClientUnblockedHandler> logger) : IInboxEventHandler<ClientUnblocked>
{
    public async Task Handle(ClientUnblocked notification, CancellationToken cancellationToken)
    {
        if (await inboxRepository.ExistsAsync(notification.EventId, cancellationToken))
        {
            logger.LogInformation("Event {EventId} уже обработан, пропускаем", notification.EventId);
            return;
        }

        try
        {
            var account = await accountRepository.GetByOwnerAsync(notification.ClientId);
            if (account != null)
            {
                logger.LogInformation("Обрабатываем ClientUnblocked для ClientId {ClientId}", notification.ClientId);
                account.Frozen = false;
                await accountRepository.UpdateAsync(account);
            }
            else
            {
                logger.LogWarning("Аккаунт не найден для ClientId {ClientId}. Пропускаем событие {EventId}.",
                    notification.ClientId, notification.EventId);
            }

            await inboxRepository.AddProcessedAsync(notification.EventId, notification.Meta.Source, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка обработки ClientUnblocked EventId {EventId}", notification.EventId);
            var deadLetter = new InboxDeadLetter
            {
                MessageId = notification.EventId,
                Handler = nameof(ClientUnblockedHandler),
                Payload = JsonSerializer.Serialize(notification),
                Error = ex.ToString(),
                CorrelationId = notification.Meta.CorrelationId,
                CausationId = notification.Meta.CausationId
            };
            await deadRepository.AddAsync(deadLetter, cancellationToken);
        }
    }
}

using MassTransit;
using RabbitMQ.Client;
using Simple_Account_Service.Features.Accounts;
using Simple_Account_Service.Features.Accounts.Events;
using Simple_Account_Service.Features.Transactions.Events;
using Simple_Account_Service.Infrastructure.Data;

namespace Simple_Account_Service.Extensions;

public static class RabbitMqMassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configurator =>
        {
            // Регистрируем консьюмеров, которые нужны для обработки сообщений
            configurator.AddConsumer<AntifraudConsumer>();
           

            // Включаем EntityFramework Outbox
            configurator.AddEntityFrameworkOutbox<SasDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            configurator.UsingRabbitMq((context, cfg) =>
            {
                // Конфигурация подключения к RabbitMQ
                cfg.Host(configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                // Объявляем exchange account.events (topic)
                cfg.Message<AccountOpened>(x => x.SetEntityName("account.events"));
                cfg.Message<MoneyCredited>(x => x.SetEntityName("account.events"));
                cfg.Message<MoneyDebited>(x => x.SetEntityName("account.events"));
                cfg.Message<TransferCompleted>(x => x.SetEntityName("account.events"));
                cfg.Message<InterestAccrued>(x => x.SetEntityName("account.events"));
                cfg.Message<ClientBlocked>(x => x.SetEntityName("account.events"));
                cfg.Message<ClientUnblocked>(x => x.SetEntityName("account.events"));

                cfg.Publish<AccountOpened>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<MoneyCredited>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<MoneyDebited>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<TransferCompleted>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<InterestAccrued>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<ClientBlocked>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<ClientUnblocked>(x => x.ExchangeType = ExchangeType.Topic);

                // Настраиваем routing key для событий
                cfg.Send<AccountOpened>(x => x.UseRoutingKeyFormatter(context => "account.opened"));
                cfg.Send<MoneyCredited>(x => x.UseRoutingKeyFormatter(context => "money.credited"));
                cfg.Send<MoneyDebited>(x => x.UseRoutingKeyFormatter(context => "money.debited"));
                cfg.Send<TransferCompleted>(x => x.UseRoutingKeyFormatter(context => "money.transfer.completed"));
                cfg.Send<InterestAccrued>(x => x.UseRoutingKeyFormatter(context => "money.interest.accrued"));
                cfg.Send<ClientBlocked>(x => x.UseRoutingKeyFormatter(context => "client.blocked"));
                cfg.Send<ClientUnblocked>(x => x.UseRoutingKeyFormatter(context => "client.unblocked"));

                // Очередь account.crm создаётся, но консьюмер не регистрируется - сообщения будут висеть в очереди.
                cfg.ReceiveEndpoint("account.crm", e =>
                {
                    e.Durable = true;
                    e.ConfigureConsumeTopology = false; // отключаем автосоздание биндингов
                    e.Bind("account.events", b =>
                    {
                        b.ExchangeType = ExchangeType.Topic;
                        b.RoutingKey = "account.*";
                        b.Durable = true;
                    });

                    e.SetQueueArgument("x-dead-letter-exchange", null);
                    e.SetQueueArgument("x-message-ttl", null);
                    e.SetQueueArgument("x-expires", null);
                    // Консьюмер не регистрируем, чтобы не забирать сообщения
                });

                // Очередь account.notifications с биндингом и обработчиком (пример, если нужен консьюмер)
                cfg.ReceiveEndpoint("account.notifications", e =>
                {
                    e.Durable = true;
                    e.ConfigureConsumeTopology = false;
                    e.Bind("account.events", b =>
                    {
                        b.ExchangeType = ExchangeType.Topic;
                        b.RoutingKey = "money.*";
                        b.Durable = true;
                    });
                    // При необходимости зарегистрировать consumer:
                    // e.ConfigureConsumer<NotificationsConsumer>(context);
                });

                // Очередь account.antifraud с биндингом и консьюмером
                cfg.ReceiveEndpoint("account.antifraud", e =>
                {
                    e.Durable = true;
                    e.ConfigureConsumeTopology = false;
                    e.Bind("account.events", b =>
                    {
                        b.ExchangeType = ExchangeType.Topic;
                        b.RoutingKey = "client.*";
                        b.Durable = true;
                    });
                    e.ConfigureConsumer<AntifraudConsumer>(context);
                });



                // Автоматическая регистрация остальных консьюмеров, если есть
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }


}
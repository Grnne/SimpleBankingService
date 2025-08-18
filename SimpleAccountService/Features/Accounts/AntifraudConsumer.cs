using MassTransit;
using Simple_Account_Service.Features.Accounts.Events;

namespace Simple_Account_Service.Features.Accounts;

public class AntifraudConsumer : IConsumer<ClientBlocked>, IConsumer<ClientUnblocked>
{
    public Task Consume(ConsumeContext<ClientBlocked> context)
    {
        throw new NotImplementedException();
    }

    public Task Consume(ConsumeContext<ClientUnblocked> context)
    {
        throw new NotImplementedException();
    }
}

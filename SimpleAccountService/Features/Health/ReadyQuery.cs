using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.Models;
using Simple_Account_Service.Infrastructure.Data;
using System.Net;

namespace Simple_Account_Service.Features.Health;

public record ReadyQuery : IRequest<MbResult<string>>;
[UsedImplicitly]
public class ReadyQueryHandler(SasDbContext dbContext) : IRequestHandler<ReadyQuery, MbResult<string>>
{
    public async Task<MbResult<string>> Handle(ReadyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var count = await dbContext.OutboxMessages.CountAsync(cancellationToken);
            if (count >= 100)
            {
                return new MbResult<string>(
                    new MbError(
                        Code: HttpStatusCode.ServiceUnavailable,
                        Message: "Сервис не готов",
                        Description: "В очереди исходящих сообщений ожидает публикация более 100 сообщений"));
            }
            return new MbResult<string>("Готов");
        }
        catch (Exception ex)
        {
            return new MbResult<string>(
                new MbError(
                    Code: HttpStatusCode.ServiceUnavailable,
                    Message: "Сервис не готов",
                    Description: ex.Message));
        }
    }
}
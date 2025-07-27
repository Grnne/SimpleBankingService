using MediatR;
using System.Diagnostics;

namespace Simple_Account_Service.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next(cancellationToken);
        stopwatch.Stop();

        logger.LogInformation("Handled {ResponseType} in {ElapsedMilliseconds}ms",
            typeof(TResponse).Name, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
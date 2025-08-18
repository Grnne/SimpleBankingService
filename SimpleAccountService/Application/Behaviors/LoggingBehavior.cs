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
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestType} with data: {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            logger.LogInformation("Handled {ResponseType} in {ElapsedMilliseconds}ms with response data: {@Response}",
                typeof(TResponse).Name, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Error handling {RequestType} after {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
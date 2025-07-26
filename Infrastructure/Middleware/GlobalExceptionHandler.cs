using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace I_wont_poop_myself_with_libs.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is FluentValidation.ValidationException fluentException)
        {
            problemDetails.Title = "one or more validation errors occurred.";
            problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7807";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var validationErrors = fluentException.Errors.Select(error => error.ErrorMessage).ToList();
            problemDetails.Extensions.Add("errors", validationErrors);
        }

        else
        {
            problemDetails.Title = exception.Message;
        }

        logger.LogError("{ProblemDetailsTitle}", problemDetails.Title);

        problemDetails.Status = httpContext.Response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
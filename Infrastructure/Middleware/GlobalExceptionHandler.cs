using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Application.Exceptions;

namespace Simple_Account_Service.Infrastructure.Middleware;

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
            problemDetails.Title = "One or more validation errors occurred.";
            problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7807";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var validationErrors = fluentException.Errors.Select(error => error.ErrorMessage).ToList();
            problemDetails.Extensions.Add("errors", validationErrors);
        }
        else if (exception is NotFoundException)
        {
            problemDetails.Title = exception.Message;
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        else if (exception is ConflictException)
        {
            problemDetails.Title = exception.Message;
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        }
        else
        {
            problemDetails.Title = "An unexpected error occurred.";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        logger.LogError("{ProblemDetailsTitle}", problemDetails.Title);

        problemDetails.Status = httpContext.Response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
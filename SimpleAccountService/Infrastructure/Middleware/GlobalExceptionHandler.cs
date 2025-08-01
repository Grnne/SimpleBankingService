using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Simple_Account_Service.Application.Exceptions;

namespace Simple_Account_Service.Infrastructure.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        ProblemDetails problemDetails;

        switch (exception)
        {
            case ValidationException validationException:
                {
                    var validationErrors = validationException.Errors;

                    problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation errors",
                        Detail = string.Join(" ", validationErrors.Select(e =>
                            $"{e.PropertyName}: {e.ErrorMessage}"))
                    };
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                }
            case ConflictException conflictException:
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict errors",
                    Detail = conflictException.Message
                };
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                break;
            case NotFoundException notFoundException:
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found errors",
                    Detail = notFoundException.Message
                };
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                break;
            default:
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error"
                };
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
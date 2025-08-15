using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Simple_Account_Service.Application.Exceptions;
using Simple_Account_Service.Application.Models;
using System.Net;

namespace Simple_Account_Service.Infrastructure.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        MbError mbError;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case ValidationException validationException:
                {
                    var errorMessages = validationException.Errors
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                    var allMessages = string.Join(" ", errorMessages);

                    mbError = new MbError(HttpStatusCode.BadRequest, "Validation errors", allMessages);
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                }
            case ConflictException conflictException:
                mbError = new MbError(HttpStatusCode.Conflict, "Conflict error", conflictException.Message);
                statusCode = HttpStatusCode.Conflict;
                break;
            case DbUpdateConcurrencyException concurrencyException:
                mbError = new MbError(HttpStatusCode.Conflict, "Concurrency exception", concurrencyException.Message);
                statusCode = HttpStatusCode.Conflict;
                break;
            case NotFoundException notFoundException:
                mbError = new MbError(HttpStatusCode.NotFound, "Not Found error", notFoundException.Message);
                statusCode = HttpStatusCode.NotFound;
                break;
            case UnauthorizedAccessException unauthorizedAccessException:
                mbError = new MbError(HttpStatusCode.Unauthorized, "Unauthorized", unauthorizedAccessException.Message);
                statusCode = HttpStatusCode.Unauthorized;
                break;
            default:
                mbError = new MbError(HttpStatusCode.InternalServerError, "Internal Server Error");
                statusCode = HttpStatusCode.InternalServerError;
                break;
        }

        var result = new MbResult<object>(mbError);

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }
}
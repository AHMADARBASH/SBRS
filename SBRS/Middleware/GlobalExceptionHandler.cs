using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SBRS.Application.Common.Exceptions;
using SBRS.Domain.Exceptions;

namespace SBRS.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        int status;
        string title;
        IDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case ValidationException ve:
                status = StatusCodes.Status400BadRequest;
                title = "Validation failed";
                errors = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;
            case NotFoundException:
                status = StatusCodes.Status404NotFound;
                title = "Not found";
                break;
            case ConflictException:
                status = StatusCodes.Status409Conflict;
                title = "Conflict";
                break;
            case DomainException:
                status = StatusCodes.Status400BadRequest;
                title = "Business rule violated";
                break;
            default:
                logger.LogError(exception, "Unhandled exception");
                status = StatusCodes.Status500InternalServerError;
                title = "An unexpected error occurred";
                break;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
        };
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
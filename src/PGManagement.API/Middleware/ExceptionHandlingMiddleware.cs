using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PGManagement.Application.Common.Exceptions;
using PGManagement.Domain.Exceptions;

namespace PGManagement.API.Middleware;

public class ExceptionHandlingMiddleware : IExceptionHandler
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(exception, "An exception occurred: {Message} | TraceId: {TraceId}", exception.Message, traceId);

        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["errors"] = validationEx.Errors,
                    ["traceId"] = traceId
                }
            },
            RoomNotFoundException or TenantNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            RoomNotAvailableException or BillAlreadyExistsException or DuplicateAllocationException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Business Rule Conflict",
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            DomainException domainEx => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain Error",
                Detail = domainEx.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please contact support with the trace identifier.",
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            }
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

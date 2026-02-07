using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace th_mm_pos.web.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, message) = MapExceptionToResponse(exception);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = message,
            statusCode = statusCode,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private (int StatusCode, string Message) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))
            ),
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                "The requested resource was not found"
            ),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Forbidden,
                "Access denied. You do not have permission to perform this action"
            ),
            DbUpdateConcurrencyException => (
                (int)HttpStatusCode.Conflict,
                "The record was modified by another user. Please refresh and try again"
            ),
            DbUpdateException => (
                (int)HttpStatusCode.BadRequest,
                "A database error occurred. Please check your input and try again"
            ),
            ArgumentException argEx => (
                (int)HttpStatusCode.BadRequest,
                argEx.Message
            ),
            InvalidOperationException invalidOpEx => (
                (int)HttpStatusCode.BadRequest,
                invalidOpEx.Message
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later"
            )
        };
    }
}
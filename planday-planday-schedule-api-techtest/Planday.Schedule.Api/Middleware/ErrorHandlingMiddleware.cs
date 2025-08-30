using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Planday.Schedule.Api.Models;

namespace Planday.Schedule.Api.Middleware;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, response) = MapException(exception);

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response, jsonOptions),
            cancellationToken
        );

        return true;
    }

    private static (HttpStatusCode StatusCode, ApiResponse<object> Response) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>
                    .Fail("Validation failed", "ValidationError")
                    .WithValidationErrors(validationEx.Errors)
            ),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(argEx.Message, "InvalidArgument")
            ),

            InvalidOperationException invEx => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse<object>.Fail(invEx.Message, "BusinessRuleViolation")
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An internal server error occurred", "InternalServerError")
            )
        };
    }
}
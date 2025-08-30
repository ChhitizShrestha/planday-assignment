using System.Net;
using System.Text.Json;
using FluentValidation;
using Planday.Schedule.Api.Models;

namespace Planday.Schedule.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("Validation failed", "ValidationError")
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

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
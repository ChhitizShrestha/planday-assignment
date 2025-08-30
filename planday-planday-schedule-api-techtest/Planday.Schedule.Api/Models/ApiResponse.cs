using FluentValidation.Results;

namespace Planday.Schedule.Api.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public IEnumerable<ValidationError>? ValidationErrors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    public static ApiResponse<T> Fail(string message, string errorCode)
    {
        return new ApiResponse<T> { Success = false, Message = message, ErrorCode = errorCode };
    }

    public ApiResponse<T> WithValidationErrors(IEnumerable<ValidationFailure> errors)
    {
        ValidationErrors = errors.Select(e => new ValidationError
        {
            PropertyName = e.PropertyName,
            ErrorMessage = e.ErrorMessage
        });
        return this;
    }
}

public class ValidationError
{
    public string PropertyName { get; set; } = default!;
    public string ErrorMessage { get; set; } = default!;
}
using Calendar.Core.Exceptions;
using Calendar.Shared.Common;
using FluentValidation;
using System.Text.Json;

namespace Calendar.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = ApiResult<object>.Fail("Đã xảy ra lỗi hệ thống.");

        switch (ex)
        {
            case ValidationException validationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Có lỗi xảy ra với dữ liệu đầu vào.";
                // In real app, would return specific validation errors
                break;
            case EntityNotFoundException notFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = notFoundException.Message;
                break;
            case AppointmentConflictException conflictException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.Message = conflictException.Message;
                break;
            case DuplicateUsernameException duplicateException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.Message = duplicateException.Message;
                break;
            case DomainException domainException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = domainException.Message;
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(result);
    }
}

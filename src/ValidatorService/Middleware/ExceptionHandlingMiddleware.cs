using System.Text.Json;
using System.Text.Json.Serialization;
using ValidatorService.Models;

namespace ValidatorService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");
            await HandleInternalServerError(context, ex);
        }
    }

    private static Task HandleInternalServerError(HttpContext context, Exception exception)
    {
        var response = new ApiResponse<object>(
            StatusCodes.Status500InternalServerError,
            error: new ErrorDetails("internal_error", "An unexpected error occurred. Please try again later.")
        );

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Excludes null fields
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Converts property names to camelCase
        };

        var jsonResponse = JsonSerializer.Serialize(response, options);
        return context.Response.WriteAsync(jsonResponse);
    }
}

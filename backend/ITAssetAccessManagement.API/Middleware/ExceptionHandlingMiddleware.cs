using System.Text.Json;

namespace ITAssetAccessManagement.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType =
            "application/json";

        var statusCode =
            StatusCodes.Status500InternalServerError;

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode =
                    StatusCodes.Status403Forbidden;
                break;

            case InvalidOperationException:
                statusCode =
                    StatusCodes.Status400BadRequest;
                break;

            case KeyNotFoundException:
                statusCode =
                    StatusCodes.Status404NotFound;
                break;
        }

        context.Response.StatusCode =
            statusCode;

        var response = new
        {
            status = statusCode,
            message = exception.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}
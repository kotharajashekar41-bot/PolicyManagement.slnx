using Microsoft.AspNetCore.Mvc;
using PolicyPlatform.Application.Common;

namespace PolicyPlatform.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppValidationException ex)
        {
            logger.LogWarning(ex, "Validation failure on {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid request", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            // Deliberately not surfacing ex.Message here: an internal 500 shouldn't
            // leak exception internals to the client. Full detail lives in the logs,
            // correlated by traceId.
            await WriteProblemAsync(
                context, StatusCodes.Status500InternalServerError, "An unexpected error occurred",
                "An unexpected error occurred. Contact support with the traceId if this persists.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        };

        context.Response.StatusCode = statusCode;
        // WriteAsJsonAsync resets ContentType to its own default unless told otherwise,
        // so the RFC7807 media type has to be passed explicitly here rather than set
        // beforehand on the response.
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}

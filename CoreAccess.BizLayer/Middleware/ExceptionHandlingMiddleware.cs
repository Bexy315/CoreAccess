using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreAccess.BizLayer.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}

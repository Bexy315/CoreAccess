using CoreAccess.BizLayer.Services;
using Microsoft.AspNetCore.Http;

namespace CoreAccess.BizLayer.Middleware;

public class InitialSetupGuardMiddleware(RequestDelegate next)
{
    private readonly string[] _allowedPaths = ["/api/setup", "/api/admin/config", "/initial-setup"];

    public async Task InvokeAsync(HttpContext context, IInitialSetupService initialSetupService)
    {
        if (!_allowedPaths.Any(p => context.Request.Path.StartsWithSegments(p)) && !initialSetupService.IsSetupCompleted())
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Initial setup required. Access denied. Please complete the setup process using the Web UI before accessing this resource.");
            return;
        }

        await next(context);
    }
}
using CoreAccess.WebAPI.Services;

namespace CoreAccess.WebAPI.Middleware;

public class InitialSetupGuardMiddleware(RequestDelegate next)
{
    private readonly string[] _allowedPaths = ["/api/setup", "/api/admin/config", "/initial-setup"];

    public async Task InvokeAsync(HttpContext context, IAppSettingsService appSettingsService)
    {
        if (!IsSetupCompleted() &&
            !_allowedPaths.Any(p => context.Request.Path.StartsWithSegments(p)) )
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Initial setup required. Access denied. Please complete the setup process using the Web UI before accessing this resource.");
            return;
        }

        await next(context);
    }
    
    private static bool IsSetupCompleted()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "/data/etc/init_setup_completed.txt");

        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath) == "true";
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, "false");
        return false;
    }
}

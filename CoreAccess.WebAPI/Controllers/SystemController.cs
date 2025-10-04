using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace CoreAccess.WebAPI.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController(CoreAccessDbContext db,IRoleService roleService,IApplicationService applicationService, ITokenService tokenService, ILogger<SystemController> logger) : ControllerBase
{
    [HttpGet("health")]
    [Produces(typeof(HealthCheckResponse))]
    public async Task<IActionResult> GetHealth()
    {
        HealthCheckResponse healthResponse = new();

        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1");
            healthResponse.Checks["Database"] = "OK";
        }
        catch (Exception ex)
        {
            healthResponse.Checks["Database"] = $"ERROR: {ex.Message}";
        }

        healthResponse.Status = healthResponse.Checks.All(kv => kv.Value?.ToString() == "OK") ? "Healthy" : "Unhealthy";
        
        if (healthResponse.Status == "Unhealthy")
        {
            Console.WriteLine("System is Unhealthy");
        }
        else
        {
            Console.WriteLine("System is Healthy");
        }
        
        healthResponse.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?? "Production";
        healthResponse.Timestamp = DateTime.UtcNow;
        healthResponse.Uptime = Environment.TickCount / 1000; 

        return Ok(healthResponse);
    }
    
    [HttpGet]
    [Route("start-debug")]
    public async Task <IActionResult> StartDebug([FromQuery]string? userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting debug...");

        if (userId == null)
            return Ok();

        var result = await tokenService.RevokeUserTokensAsync(userId, cancellationToken);
        
        return Ok(result);
    }
}

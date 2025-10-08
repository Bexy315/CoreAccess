using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace CoreAccess.WebAPI.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController(CoreAccessDbContext db,IRoleService roleService, IRoleRepository roleRepository, IApplicationService applicationService, ITokenService tokenService, ILogger<SystemController> logger) : ControllerBase
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
    public async Task <IActionResult> StartDebug(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting debug...");

        var result = await roleRepository.SearchRolesAsync(new RoleSearchOptions()
        {
            IncludeUsers = true,
            IncludePermissions = true,
            Page = 1,
            PageSize = 100
        }, cancellationToken);
        return Ok(result);
    }
}

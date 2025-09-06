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
public class SystemController(IAppSettingsService appSettingsService, IOpenIddictService openIddictService, CoreAccessDbContext db) : ControllerBase
{
    [HttpGet("health")]
    [Produces(typeof(HealthCheckResponse))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
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

        var tokenKey = appSettingsService.Get(AppSettingsKeys.JwtSecretKey, decryptIfNeeded: true);
        healthResponse.Checks["JwtSecretKey"] = string.IsNullOrWhiteSpace(tokenKey) ? "MISSING" : "OK";

        healthResponse.Status = healthResponse.Checks.All(kv => kv.Value?.ToString() == "OK") ? "Healthy" : "Unhealthy";
        
        if (healthResponse.Status == "Unhealthy")
        {
            Console.WriteLine("System is Unhealthy");
        }
        else
        {
            Console.WriteLine("System is Healthy");
        }
        
        healthResponse.Timestamp = DateTime.UtcNow;
        healthResponse.Uptime = Environment.TickCount / 1000; // Convert milliseconds to seconds

        return Ok(healthResponse);
    }
    
    [HttpGet]
    [Route("start-debug")]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task <IActionResult> StartDebug(CancellationToken cancellationToken = default)
    {
        try
        {
            //appSettingsService.TryGet(AppSettingsKeys.SystemLogLevel, out string? systemLogLevel);
            await openIddictService.AddApplicationAsync(new OpenIddictApplicationDescriptor()
            {
                ClientId = "test-client",
                DisplayName = "Test Client",
                Permissions =
                {
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                }
            });
            
            return Ok();
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}

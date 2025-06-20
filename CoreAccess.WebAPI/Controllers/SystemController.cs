using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController(IAppSettingsService appSettingsService, CoreAccessDbContext db) : ControllerBase
{
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        var result = new Dictionary<string, object>();

        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1");
            result["Database"] = "OK";
        }
        catch (Exception ex)
        {
            result["Database"] = $"ERROR: {ex.Message}";
        }

        var tokenKey = appSettingsService.Get(AppSettingsKeys.JwtSecretKey, decryptIfNeeded: true);
        result["JwtSecretKey"] = string.IsNullOrWhiteSpace(tokenKey) ? "MISSING" : "OK";

        var overallStatus = result.All(kv => kv.Value?.ToString() == "OK") ? "Healthy" : "Unhealthy";

        return Ok(new
        {
            Status = overallStatus,
            Uptime = Environment.TickCount64 / 1000,
            Checks = result
        });
    }
    
    [HttpGet]
    [Route("start-debug")]
    public async Task <IActionResult> StartDebug(CancellationToken cancellationToken = default)
    {
        try
        {
            appSettingsService.TryGet(AppSettingsKeys.SystemLogLevel, out string? systemLogLevel);
            
            CoreLogger.LogSystem(CoreLogLevel.Information,nameof(SystemController), "Cool Debug message!!!", new Exception("Test exception"));
            
            return Ok(systemLogLevel);
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
    
    [HttpGet]
    [Route("ping")]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }
}

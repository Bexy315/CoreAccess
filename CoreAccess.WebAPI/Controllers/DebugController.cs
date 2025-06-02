using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/debug")]
public class DebugController() : ControllerBase
{
    [HttpGet]
    [Route("start-debug")]
    public async Task <IActionResult> StartDebug(CancellationToken cancellationToken = default)
    {
        try
        {
            AppSettingsHelper.TryGet(AppSettingsKeys.SystemLogLevel, out string? systemLogLevel);
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
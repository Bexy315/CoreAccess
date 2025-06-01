using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/debug")]
public class DebugController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    [Route("start-debug")]
    public async Task <IActionResult> StartDebug(CancellationToken cancellationToken = default)
    {
        try
        {
            var config = configuration["Jwt:Issuer"];
            return Ok(config);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error starting debug mode: {ex.Message}");
        }
    }
    [HttpGet]
    [Route("ping")]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }
}
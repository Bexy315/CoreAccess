using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/setup")]
public class InitialSetupController(IInitialSetupService initialSetupService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RunSetupAsync([FromBody] InitialSetupRequest request)
    {
        try
        {
            await initialSetupService.RunSetupAsync(request);
            return Ok("Setup completed successfully.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Route("api/setup")]
public class InitialSetupController(InitialSetupService initialSetupService) : ControllerBase
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
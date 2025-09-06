using CoreAccess.BizLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Controller]
[Route("api/admin")]
public class AdminController(IInitialSetupService initialSetupService) : ControllerBase
{
    [HttpGet]
    [Route("config")]
    [Produces(typeof(bool))]
    public async Task<IActionResult> GetAppConfig(CancellationToken cancellationToken = default)
    {
        try
        {
            var isSetupComplete = initialSetupService.IsSetupCompleted();
            return Ok(new { IsSetupComplete = isSetupComplete });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
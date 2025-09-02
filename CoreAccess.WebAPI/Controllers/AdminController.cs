using CoreAccess.BizLayer.Logger;
using CoreAccess.BizLayer.Services;
using CoreAccess.WebAPI.Logger.Model;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(AdminController), "Error while checking initial setup status", ex);
            return StatusCode(500, ex.Message);
        }
    }
}
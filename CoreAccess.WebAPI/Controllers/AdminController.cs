using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;
[Controller]
[Route("api/admin")]
public class AdminController( IUserService userService, InitialSetupService initialSetupService) : ControllerBase
{
    [HttpGet]
    [Route("config")]
    public async Task<IActionResult> GetAppConfig(CancellationToken cancellationToken = default)
    {
        try
        {
            var isSetupComplete = await initialSetupService.IsSetupCompletedAsync(cancellationToken);
            return Ok(new { IsSetupComplete = isSetupComplete });
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(AdminController), "Error while checking initial setup status", ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet]
    [Route("user")]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetUser([FromQuery]CoreUserSearchOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.SearchUsersAsync(options, cancellationToken);

            return Ok(user);
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while getting user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while getting user", ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpDelete]
    [Route("user/{userId}")]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> DeleteUser([FromRoute]string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await userService.DeleteUserAsync(userId, cancellationToken);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while deleting user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while deleting user", ex);
            return StatusCode(500, ex.Message);
        }
    }
        
    [HttpPost]
    [Route("user/{userId}/role/{roleName}")]
    [CoreAuthorize]
    public async Task<IActionResult> AddRoleToUser([FromRoute]string userId, [FromRoute]string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            await userService.AddRoleToUserAsync(userId, roleName, cancellationToken);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while adding role to user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while adding role to user", ex);
            return StatusCode(500, ex.Message);
        }
    }
}
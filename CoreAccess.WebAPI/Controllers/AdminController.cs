using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;
[Controller]
[Route("api/admin")]
public class AdminController( IUserService userService) : ControllerBase
{
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while getting user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while getting user", ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpDelete]
    [Route("{userId}")]
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while deleting user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while deleting user", ex);
            return StatusCode(500, ex.Message);
        }
    }
}
using CoreAccess.BizLayer.Logger;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using CoreAccess.WebAPI.Controllers;
using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;
[Controller]
[Route("api/admin")]
public class AdminController(IUserService userService, IInitialSetupService initialSetupService) : ControllerBase
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
    
    [HttpGet]
    [Route("user")]
    [Produces(typeof(PagedResult<UserDto>))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetUser([FromQuery]UserSearchOptions options, CancellationToken cancellationToken = default)
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
    
    [HttpPost]
    [Route("user")]
    [Produces(typeof(UserDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> CreateUser([FromBody]UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return BadRequest("User request cannot be null.");
            
            var user = await userService.CreateUserAsync(request, cancellationToken);
            return Ok(user);
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while creating user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while creating user", ex);
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut]
    [Route("user/{userId}")]
    [Produces(typeof(UserDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> UpdateUser([FromRoute]string userId, [FromBody]UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return BadRequest("User request cannot be null.");
            
            var user = await userService.UpdateUserAsync(userId, request, cancellationToken);
            return Ok(user);
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while updating user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while updating user", ex);
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
    [Produces(typeof(UserDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> AddRoleToUser([FromRoute]string userId, [FromRoute]string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.AddRoleToUserAsync(userId, roleName, cancellationToken);
            return Ok(user);
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
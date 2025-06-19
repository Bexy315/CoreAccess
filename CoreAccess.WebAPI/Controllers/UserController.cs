using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Route("{userId}/profile-picture")]
    [CoreAuthorize]
    public async Task<IActionResult> GetProfilePicture([FromRoute]string userId)
    {
        try
        {
            var user = await userService.GetUserByIdAsync(userId);
            
            if(user.ProfilePicture == null || user.ProfilePicture.Length == 0)
                return NotFound("Profile picture not found.");
            
            return File(user.ProfilePicture, user.ProfilePictureContentType??"image/jpeg");
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while getting profile picture", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while getting profile picture", ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [CoreAuthorize]
    public async Task<IActionResult> CreateUser([FromBody]CoreUserCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.CreateUserAsync(request, cancellationToken);
            
            if (user == null)
                return BadRequest("User creation failed.");
            
            return Ok(new
            {
                userId = user.Id,
            });
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while creating user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while creating user", ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Route("{userId}")]
    [CoreAuthorize]
    public async Task<IActionResult> UpdateUser([FromRoute]string userId, [FromBody]CoreUserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(userId))
                return BadRequest("User ID is required.");
            
            return Ok(new CoreUserDto(await userService.UpdateUserAsync(userId, request, cancellationToken)));
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while updating user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while updating user", ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost]
    [Route("{userId}/profile-picture")]
    [CoreAuthorize]
    public async Task<IActionResult> UploadProfilePicture([FromRoute]string userId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            
            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Uploaded file is not an image.");
            
            await userService.UpdateUserProfilePicutre(userId, file, HttpContext.RequestAborted);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while uploading profile picture", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while uploading profile picture", ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost]
    [Route("{userId}/role/{roleName}")]
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
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while adding role to user", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(UserController), "Error while adding role to user", ex);
            return StatusCode(500, ex.Message);
        }
    }
}
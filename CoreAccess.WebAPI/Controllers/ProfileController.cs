using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Logger;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/profile")]
public class ProfileController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Route("{userId}")]
    [Produces(typeof(UserDto))]
    [CoreAuthorize]
    public async Task<IActionResult> GetProfile([FromRoute]string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(userId))
                return BadRequest("User ID is required.");
            
            var user = await userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
                return NotFound("User not found.");
            
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
    
    [HttpGet]
    [Route("{userId}/profile-picture")]
    [Produces("image/jpeg", "image/png", "image/gif")]
    [CoreAuthorize]
    public async Task<IActionResult> GetProfilePicture([FromRoute]string userId)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(userId))
                return BadRequest("User ID is required.");
            
            var user = await userService.GetUserByIdAsync(userId);
            
            if(user.ProfilePicture == null || user.ProfilePicture.Length == 0)
                return NotFound("Profile picture not found.");
            
            return File(user.ProfilePicture, user.ProfilePictureContentType??"image/jpeg");
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while getting profile picture", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while getting profile picture", ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Route("{userId}")]
    [Produces(typeof(UserDto))]
    [CoreAuthorize]
    public async Task<IActionResult> UpdateProfile([FromRoute]string userId, [FromBody]UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(userId))
                return BadRequest("User ID is required.");
            
            if(userId != User.FindFirst(claim => (claim.Type == AccessClaimType.UserId))?.Value)
                return Forbid("You can only update your own profile.");
             
            return Ok(await userService.UpdateUserAsync(userId, request, cancellationToken));
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
    
    [HttpPost]
    [Route("{userId}/profile-picture")]
    [CoreAuthorize]
    public async Task<IActionResult> UploadProfilePicture([FromRoute]string userId, IFormFile file)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(userId))
                return BadRequest("User ID is required.");
            
            if(userId != User.FindFirst(claim => (claim.Type == AccessClaimType.UserId))?.Value)
                return Forbid("You can only update your own profile picture.");
            
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            
            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Uploaded file is not an image.");
            
            await userService.UpdateUserProfilePicutre(userId, file, HttpContext.RequestAborted);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while uploading profile picture", ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(ProfileController), "Error while uploading profile picture", ex);
            return StatusCode(500, ex.Message);
        }
    }
}
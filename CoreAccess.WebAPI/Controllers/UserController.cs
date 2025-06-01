using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("/api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [CoreAuthorize]
    public async Task<IActionResult> GetUser([FromQuery]CoreUserSearchOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.SearchUsersAsync(options, cancellationToken);

            return Ok(user);
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet]
    [Route("api/users/{userId}/profile-picture")]
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
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    [CoreAuthorize]
    public async Task<IActionResult> CreateUser([FromBody]CoreUserCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await userService.CreateUserAsync(request, cancellationToken));
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/user/{userId}")]
    [CoreAuthorize]
    public async Task<IActionResult> UpdateUser([FromRoute]string userId, [FromBody]CoreUserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await userService.UpdateUserAsync(userId, request, cancellationToken));
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost]
    [Route("api/users/{userId}/profile-picture")]
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
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost]
    [Route("/api/user/{userId}/role/{roleName}")]
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
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete]
    [Route("/api/user/{userId}")]
    [CoreAuthorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser([FromRoute]string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await userService.DeleteUserAsync(userId, cancellationToken);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}
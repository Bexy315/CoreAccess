using CoreAccess.BizLayer.Decorator;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
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
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet]
    [Route("{userId}")]
    [Produces(typeof(PagedResult<UserDto>))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> GetUserById([FromRoute]string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if(string.IsNullOrEmpty(userId))
                return BadRequest("User ID cannot be null or empty.");
            
            var user = await userService.GetUserByIdAsync(userId, cancellationToken);
            if(user == null)
                return NotFound($"User with ID {userId} not found.");
            
            return Ok(user);
        }
        catch(ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost]
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
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPut]
    [Route("{userId}")]
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
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
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
            if(string.IsNullOrEmpty(userId))
                return BadRequest("User ID cannot be null or empty.");
            
            await userService.DeleteUserAsync(userId, cancellationToken);
            return Ok();
        }
        catch(ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
        
    [HttpPost]
    [Route("{userId}/role/{roleId}")]
    [Produces(typeof(UserDto))]
    [CoreAuthorize(Roles = "CoreAccess.Admin")]
    public async Task<IActionResult> AddRoleToUser([FromRoute]string userId, [FromRoute]string roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId))
                return BadRequest("User ID and Role ID cannot be null or empty.");
            
            var user = await userService.AddRoleToUserAsync(userId, roleId, cancellationToken);
            return Ok(user);
        }
        catch(ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
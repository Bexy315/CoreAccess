using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("/api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery]CoreUserSearchOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.SearchUsersAsync(options, cancellationToken);

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> CreateUser([FromBody]CoreUserCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await userService.CreateUserAsync(request, cancellationToken));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/user/{userId}")]
    public async Task<IActionResult> UpdateUser([FromRoute]string userId, [FromBody]CoreUserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await userService.UpdateUserAsync(userId, request, cancellationToken));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [Route("/api/user/{userId}")]
    public async Task<IActionResult> DeleteUser([FromRoute]string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await userService.DeleteUserAsync(userId, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
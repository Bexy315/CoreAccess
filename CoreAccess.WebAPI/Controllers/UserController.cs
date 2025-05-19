using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("/api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery]CoreUserSearchOptions options)
    {
        try
        {
            var user = await userService.SearchUsersAsync(options);

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> CreateUser([FromBody]CoreUserCreateRequest request)
    {
        try
        {
            return Ok(await userService.CreateUserAsync(request));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("/{userId}")]
    public async Task<IActionResult> UpdateUser([FromRoute]string userId, [FromBody]CoreUserUpdateRequest request)
    {
        try
        {
            return Ok(await userService.UpdateUserAsync(userId, request));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [Route("/{userId}")]
    public async Task<IActionResult> DeleteUser([FromRoute]string userId)
    {
        try
        {
            await userService.DeleteUserAsync(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
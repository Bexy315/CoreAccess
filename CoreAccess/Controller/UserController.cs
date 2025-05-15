using CoreAccess.Model;
using CoreAccess.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.Controller;

[Controller]
[Route("/api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUser([FromQuery]string? id, [FromQuery] string? userName)
    {
        try
        {
            var user = await userService.SearchUsersAsync(new CoreUserSearchOptions()
            {
                Id = id,
                Name = userName
            });

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
            return Ok(await userService.DeleteUserAsync(userId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAccess.WebAPI.Controllers;

public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
      //  var users = await _userService.GetAllUsersAsync();
        return Ok("Not Implemented");
    }
}
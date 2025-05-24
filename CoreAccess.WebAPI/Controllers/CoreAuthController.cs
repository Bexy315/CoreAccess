using Microsoft.AspNetCore.Mvc;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;

namespace CoreAccess.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoreAuthController(IUserService userService, ICoreAccessTokenService tokenService)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CoreUserCreateRequest dto)
    {
        if (await userService.UsernameExistsAsync(dto.Username))
            return BadRequest("Username already exists.");

        var user = await userService.CreateUserAsync(dto);
        return Ok(new { user.Id, user.Username });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] CoreLoginRequest dto)
    {
        if(dto.Username == null && dto.Email == null)
            return BadRequest("Either Username or email is required.");
        
        var user = await userService.ValidateCredentialsAsync(dto.Username, dto.Email, dto.Password);
        if (user == null)
            return Unauthorized("Invalid credentials.");

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user, dto.LoginIp);

        return Ok(new
        {
            access_token = accessToken,
            refresh_token = refreshToken
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] CoreRefreshTokenRequest dto)
    {
        var user = await userService.GetUserByRefreshTokenAsync(dto.RefreshToken);
        if (user == null)
            return Unauthorized("Invalid refresh token.");

        var newAccessToken = await tokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user, dto.LoginIp);

        return Ok(new
        {
            access_token = newAccessToken,
            refresh_token = newRefreshToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] CoreRefreshTokenRequest dto)
    {
        await tokenService.RevokeRefreshTokenAsync(dto.RefreshToken);
        return Ok();
    }
}

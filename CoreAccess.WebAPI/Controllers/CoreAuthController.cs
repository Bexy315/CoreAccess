using Microsoft.AspNetCore.Mvc;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/auth")]
public class CoreAuthController(IUserService userService, ICoreAccessTokenService tokenService) : ControllerBase
{
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] CoreUserCreateRequest dto, CancellationToken cancellationToken = default)
    {
        if (await userService.UsernameExistsAsync(dto.Username, cancellationToken))
            return BadRequest("Username already exists.");

        var user = await userService.CreateUserAsync(dto, cancellationToken);
        return Ok(new { user.Id, user.Username });
    }
    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] CoreLoginRequest dto, CancellationToken cancellationToken = default)
    {
        if(dto.Username == null)
            return BadRequest("Username or email is required.");
        
        var user = await userService.ValidateCredentialsByUsernameAsync(dto.Username, dto.Password, cancellationToken);
        if (user == null)
            return Unauthorized("Invalid credentials.");

        var accessToken = await tokenService.GenerateAccessTokenAsync(user , cancellationToken: cancellationToken);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user, dto.LoginIp, cancellationToken);

        return Ok(new
        {
            access_token = accessToken,
            refresh_token = refreshToken
        });
    }
    [HttpPost("/refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] CoreRefreshTokenRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await tokenService.ValidateRefreshToken(dto.RefreshToken, cancellationToken);

            if (user == null)
                return Unauthorized("Invalid refresh token.");
            
            var newAccessToken =
                await tokenService.GenerateAccessTokenAsync(user, cancellationToken: cancellationToken);
            var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user, dto.LoginIp, cancellationToken);

            await tokenService.RevokeRefreshTokenAsync(dto.RefreshToken, dto.LoginIp, cancellationToken);

            await tokenService.RecycleRefreshTokensAsync(cancellationToken);
            
            return Ok(new
            {
                access_token = newAccessToken,
                refresh_token = newRefreshToken
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message + " " + e.InnerException?.Message);
        }
    }
    [HttpPost("/logout")]
    public async Task<IActionResult> Logout([FromBody] CoreRefreshTokenRequest dto, CancellationToken cancellationToken = default)
    {
        await tokenService.RevokeRefreshTokenAsync(dto.RefreshToken, dto.LoginIp, cancellationToken: cancellationToken);
        return Ok();
    }
}

using CoreAccess.WebAPI.Decorator;
using CoreAccess.WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using CoreAccess.WebAPI.Services.CoreAuth;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/auth")]
public class CoreAuthController(IAppSettingsService appSettingsService, IUserService userService, ICoreAccessTokenService tokenService) : ControllerBase
{
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] CoreLoginRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.ValidateCredentialsByUsernameAsync(dto.Username, dto.Password,
                cancellationToken);

            var accessToken = tokenService.GenerateAccessToken(user, cancellationToken: cancellationToken);
            var refreshToken = await tokenService.GenerateRefreshTokenAsync(user, dto.LoginIp, cancellationToken);

            return Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshToken.Token,
                userId = user.Id
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] CoreRefreshTokenRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
           return Ok(await tokenService.RefreshTokenAsync(dto, cancellationToken));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] CoreRefreshTokenRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            await tokenService.RevokeRefreshTokenAsync(dto.RefreshToken, dto.LoginIp,
                cancellationToken: cancellationToken);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CoreRegisterRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (appSettingsService.Get(AppSettingsKeys.DisableRegistration) == "true")
                return StatusCode(403, "Registration is disabled.");
            
            if (await userService.UsernameExistsAsync(dto.Username, cancellationToken))
                return BadRequest("Username already exists.");

            var user = await userService.CreateUserAsync(new CoreUserCreateRequest(){Password = dto.Password, Username = dto.Username}, cancellationToken);
            return Ok(user.Id);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}
